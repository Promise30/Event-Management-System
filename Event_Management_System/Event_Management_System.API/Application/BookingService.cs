using AutoMapper;
using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;
using Event_Management_System.API.Infrastructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Net;
using System.Security.Cryptography;
using Error = Event_Management_System.API.Helpers.Error;
namespace Event_Management_System.API.Application
{
    public class BookingService : IBookingService
    {
        // - cancel a booking

        // as an event organizer, you should have the ability to perform the following actions:
        // 1. create a booking for an event centre. This allows you to reserve a venue for your event.
        // 2. View all booking. This can be viewed in two different ways: organizer can view all bookings they have made
        //  - admin can view all bookings made on the system
        // to better support this features, there should be filters to view bookings by event centre, date range, and booking status.
        // the number of endpoints for this features will be 

        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BookingService> _logger;
        private readonly IMapper _mapper;

        public BookingService(ApplicationDbContext dbContext, ILogger<BookingService> logger, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
        }
        public async Task<APIResponse<BookingDto>> CreateBooking(Guid userId, CreateBookingDto createBookingDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<BookingDto>.Create(HttpStatusCode.BadRequest, "User does not exist", null);

            var eventCentre = await _dbContext.EventCentres
                .Include(ec=> ec.Availabilities)
                .FirstOrDefaultAsync(e => e.Id == createBookingDto.EventCentreId);

            if (eventCentre == null)
                return APIResponse<BookingDto>.Create(HttpStatusCode.NotFound, "Event centre does not exist", null, new Error { Message = "Event centre does not exist" });

            if (!eventCentre.IsActive)
                return APIResponse<BookingDto>.Create(HttpStatusCode.BadRequest, "Event centre is not active", null, new Error { Message = "Event centre is not active" });

            if (!eventCentre.Availabilities.Any())
                return APIResponse<BookingDto>.Create(HttpStatusCode.BadRequest, "Event centre has no availability periods defined", null, new Error { Message = "Event centre has no availability periods defined" });

            // Validate booking falls within event centre availability periods defined
            var availabilityValidation = ValidateBookingAgainstEventCentreAvailability(createBookingDto.BookedFrom, createBookingDto.BookedTo, eventCentre.Availabilities.ToList());
            if (!availabilityValidation.IsValid)
                return APIResponse<BookingDto>.Create(HttpStatusCode.BadRequest, availabilityValidation.ErrorMessage, null, new Error { Message = availabilityValidation.ErrorMessage });

            // Check for booking conflicts
            var isAvailable = CheckEventCentreAvailability(createBookingDto.EventCentreId, createBookingDto.BookedFrom, createBookingDto.BookedTo);
            if (!isAvailable)
            {
                return APIResponse<BookingDto>.Create(HttpStatusCode.BadRequest, "The event centre is not available for the specified date and time", null, new Error { Message = "The event centre is not available for the specified date and time" });
            }
            var bookingRecord = new Booking
            {
                EventCentreId = createBookingDto.EventCentreId,
                BookingStatus = BookingStatus.Submitted,
                BookedFrom = createBookingDto.BookedFrom,
                BookedTo = createBookingDto.BookedTo,
                OrganizerId = user.Id,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.Bookings.Add(bookingRecord);

            var auditLog = new AuditLog
            {
                ObjectId = bookingRecord.Id,
                ActionType = ActionType.Create,
                UserId = user.Id,
                Description = $"Added new booking for event center {eventCentre.Name} from {bookingRecord.BookedFrom} to {bookingRecord.BookedTo} at {DateTimeOffset.UtcNow}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();

            var response = _mapper.Map<BookingDto>(bookingRecord);
            return APIResponse<BookingDto>.Create(HttpStatusCode.Created, "Request successful", response);

        }
        public async Task<APIResponse<PagedList<BookingDto>>> GetAllBookings(Guid userId, RequestParameters requestParameters, BookingFilter bookingFilter)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<PagedList<BookingDto>>.Create(HttpStatusCode.BadRequest, "User does not exist", null, new Error { Message = "User does not exist" });
            
            IQueryable<Booking> query = _dbContext.Bookings
                .Include(b => b.EventCentre)
                .Include(b => b.Organizer)
                .AsQueryable();

            // Filter records based on role type: for admins, return all booking records in the system while for organizers, return their bookings
            //var isAdmin = await _userManager.IsInRoleAsync(user, "Administrator");
            //if (!isAdmin)
            //{
            //    query = query.Where(b => b.Organizer.Id == userId);
            //}
            // Filter by booking status
            if (bookingFilter.BookingStatus.HasValue)
            {
                query = query.Where(b => b.BookingStatus == bookingFilter.BookingStatus);
            }
            // Filter by eventCenterId
            if (bookingFilter.EventCentreId != null || bookingFilter.EventCentreId.HasValue)
            {
                query = query.Where(b => b.EventCentreId == bookingFilter.EventCentreId);
            }
            // Filter by the date filters
            if (bookingFilter.StartDate.HasValue && bookingFilter.EndDate.HasValue)
            {
                query = query.Where(b => b.BookedFrom >= bookingFilter.StartDate && b.BookedTo <= bookingFilter.EndDate);
            }
            else if (bookingFilter.StartDate.HasValue)
            {
                query = query.Where(b => b.BookedFrom >= bookingFilter.StartDate);
            }
            else if (bookingFilter.EndDate.HasValue)
            {
                query = query.Where(b => b.BookedTo <= bookingFilter.EndDate);
            }
            query = query.OrderByDescending(b => b.CreatedDate);

            // Use PagedList.ToPagedList method to apply pagination
            var pagedBookings = await PagedList<Booking>.ToPagedList(query, null, requestParameters);


            if (pagedBookings.Data == null || pagedBookings.Data.Count == 0)
                return APIResponse<PagedList<BookingDto>>.Create(HttpStatusCode.OK, "No booking records found",
                  new PagedList<BookingDto>(new List<BookingDto>(), 0, requestParameters.PageNumber, requestParameters.PageSize));


            var bookingDtos = pagedBookings.Data.Select(booking =>_mapper.Map<BookingDto>(booking)).ToList();
            var response = new PagedList<BookingDto>(bookingDtos, pagedBookings.MetaData.TotalCount,
                pagedBookings.MetaData.CurrentPage, pagedBookings.MetaData.PageSize);

            return APIResponse<PagedList<BookingDto>>.Create(HttpStatusCode.OK, "Request successful", response);
        }

        public async Task<APIResponse<object>> GetBookingById(Guid bookingId)
        {
            var booking = await _dbContext.Bookings
                .Include(b => b.EventCentre)
                .Include(b => b.Organizer)
                .FirstOrDefaultAsync(b => b.Id.Equals(bookingId));
            if (booking == null)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Booking record does not exist", null, new Error { Message = "Booking record does not exist" });
            var response = _mapper.Map<BookingDto>(booking);
            return APIResponse<object>.Create(HttpStatusCode.OK, "Request successful", response);
        }

        // Redundant
        public async Task<APIResponse<object>> GetBookingsByUser(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User does not exist", null);
            var bookings = await _dbContext.Bookings
                .Include(b => b.EventCentre)
                .Where(b => b.OrganizerId.Equals(user.Id))
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
            if (bookings == null || bookings.Count == 0)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "No booking records found", null, new Error { Message = "No booking records found" });
            var response = bookings.Select(booking => _mapper.Map<BookingDto>(booking)).ToList();
            return APIResponse<object>.Create(HttpStatusCode.OK, "Request successful", response);
        }

        public async Task<APIResponse<object>> UpdateBookingStatus(Guid userId, BookingStatus bookingStatus, Guid bookingId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User does not exist", null);
            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id.Equals(bookingId));
            if (booking == null)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Booking record does not exist", null, new Error { Message = "Booking record does not exist" });
            // Check if user is authorized (admin or booking organizer)
            var isAdmin = await _userManager.IsInRoleAsync(user, "Administrator");
            if (!isAdmin && booking.OrganizerId != userId)
                return APIResponse<object>.Create(HttpStatusCode.Forbidden, "You are not authorized to update this booking", null, new Error { Message = "You are not authorized to update this booking" });

            // Prevent updating cancelled or rejected bookings
            if (booking.BookingStatus == BookingStatus.Cancelled || booking.BookingStatus == BookingStatus.Rejected)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "Cannot update a cancelled or rejected booking", null, new Error { Message = "Cannot update a cancelled or rejected booking" });

            if (booking.BookingStatus == bookingStatus)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "The booking is already in the specified status", null, new Error { Message = "The booking is already in the specified status" });
            booking.BookingStatus = bookingStatus;
            booking.ModifiedDate = DateTimeOffset.UtcNow;
            var auditLog = new AuditLog
            {
                ObjectId = booking.Id,
                ActionType = ActionType.Update,
                UserId = user.Id,
                Description = $"Updated booking status for event center {booking.EventCentreId} from {booking.BookedFrom} to {booking.BookedTo} to {bookingStatus.GetEnumDescription()} at {DateTimeOffset.UtcNow}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
            return APIResponse<object>.Create(HttpStatusCode.OK, "Request successful", null);

        }
        public async Task<APIResponse<object>> DeleteBookingAsync(Guid userId, Guid bookingId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User does not exist", Enumerable.Empty<object>(), new Error { Message = "User does not exist" });
            var booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Booking does not exist", Enumerable.Empty<object>(), new Error { Message = "Booking does not exist" });

            _dbContext.Bookings.Remove(booking);

            var auditLog = new AuditLog
            {
                ObjectId = userId,
                ActionType = ActionType.Delete,
                Description = $"Deleted booking record with id {bookingId} by {user.Email} at {DateTimeOffset.UtcNow}",
                CreatedDate = DateTimeOffset.UtcNow,
                UserId = userId,
                ModifiedDate = DateTimeOffset.UtcNow,
            };
            await _dbContext.AuditLogs.AddAsync(auditLog);
            await _dbContext.SaveChangesAsync();

            return APIResponse<object>.Create(HttpStatusCode.NoContent, "Request successful", Enumerable.Empty<object>());
        }
        public async Task<APIResponse<object>> GetAvailableEventCentres(DateTime bookedFrom, DateTime bookedTo)
        {
            // Get all active event centres with their availabilities
            var eventCentres = await _dbContext.EventCentres
                .Include(ec => ec.Availabilities)
                .Where(ec => ec.IsActive)
                .ToListAsync();

            // Get IDs of event centres that are booked during the specified time range
            var bookedEventCentreIds = await _dbContext.Bookings
                .Where(b => b.BookingStatus != BookingStatus.Cancelled
                         && b.BookingStatus != BookingStatus.Rejected
                         && b.BookedFrom < bookedTo
                         && b.BookedTo > bookedFrom)
                .Select(b => b.EventCentreId)
                .Distinct()
                .ToListAsync();

            // Filter event centres based on:
            // 1. Not currently booked
            // 2. Has availability periods defined
            // 3. Booking falls within availability periods
            var availableEventCentres = eventCentres
                .Where(ec => !bookedEventCentreIds.Contains(ec.Id)
                          && ec.Availabilities.Any()
                          && ValidateBookingAgainstEventCentreAvailability(bookedFrom, bookedTo, ec.Availabilities.ToList()).IsValid)
                .Select(ec => new EventCentreDto
                {
                    Id = ec.Id,
                    Name = ec.Name,
                    Location = ec.Location,
                    Capacity = ec.Capacity,
                    Description = ec.Description,
                    Availabilities = ec.Availabilities.Select(a => new EventCentreAvailabilityDto
                    {
                        Id = a.Id,
                        Day = a.Day.GetEnumDescription(),
                        OpenTime = a.OpenTime,
                        CloseTime = a.CloseTime
                    }).ToList()
                })
                .ToList();

            if (availableEventCentres.Count == 0)
                return APIResponse<object>.Create(
                    HttpStatusCode.NotFound,
                    "No available event centres found for the specified date and time",
                    null,
                    new Error { Message = "No available event centres found for the specified date and time" });

            return APIResponse<object>.Create(HttpStatusCode.OK, "Request successful", availableEventCentres);
        }

        #region Private methods
        private bool CheckEventCentreAvailability(Guid eventCentreId, DateTime bookedFrom, DateTime bookedTo)
        {
            bool hasConflict = _dbContext.Bookings
                .Where(b => b.BookingStatus != BookingStatus.Cancelled && b.BookingStatus != BookingStatus.Rejected)
                .Any(b => b.EventCentreId == eventCentreId
                       && b.BookedFrom < bookedTo
                       && b.BookedTo > bookedFrom);

            return !hasConflict; // Returns true if available (no conflict)
        }

        // Event Centre Availability validation against New Booking method
        private (bool IsValid, string ErrorMessage) ValidateBookingAgainstEventCentreAvailability(
            DateTime bookedFrom,
            DateTime bookedTo,
            List<EventCentreAvailability> availabilities)
        {
            // Booking must be in the future
            if (bookedFrom <= DateTime.Now)
                return (false, "Booking start time must be in the future");

            // Booking end must be after start
            if (bookedTo <= bookedFrom)
                return (false, "Booking end time must be after start time");

            // Check if booking spans multiple days
            var currentDate = bookedFrom.Date;
            var endDate = bookedTo.Date;
            // Validate each day in the booking range
            while (currentDate <= endDate)
            {
                var dayOfWeek = currentDate.DayOfWeek;

                // Get availability for this day
                var dayAvailability = availabilities
                    .Where(a => a.Day == dayOfWeek)
                    .OrderBy(a => a.OpenTime)
                    .ToList();

                 if (!dayAvailability.Any())
                    return (false, $"Event centre is not available on {dayOfWeek}s");

                // Determine the time range to check for this specific day
                TimeSpan bookingStartTime;
                TimeSpan bookingEndTime;

                if (currentDate == bookedFrom.Date && currentDate == bookedTo.Date)
                {
                    // Single day booking
                    bookingStartTime = bookedFrom.TimeOfDay;
                    bookingEndTime = bookedTo.TimeOfDay;
                }
                else if (currentDate == bookedFrom.Date)
                {
                    // First day of multi-day booking
                    bookingStartTime = bookedFrom.TimeOfDay;
                    bookingEndTime = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59));
                }

                else if (currentDate == bookedTo.Date)
                {
                    // Last day of multi-day booking
                    bookingStartTime = TimeSpan.Zero;
                    bookingEndTime = bookedTo.TimeOfDay;
                }
                else
                {
                    // Middle day of multi-day booking (full day)
                    bookingStartTime = TimeSpan.Zero;
                    bookingEndTime = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59));
                }
                // Check if booking times fall within any availability window for this day
                bool isWithinAvailability = dayAvailability.Any(a =>
                    bookingStartTime >= a.OpenTime && bookingEndTime <= a.CloseTime);

                if (!isWithinAvailability)
                {
                    // Try to provide helpful error message
                    var availableSlots = string.Join(", ", dayAvailability.Select(a => $"{a.OpenTime:hh\\:mm} - {a.CloseTime:hh\\:mm}"));
                    return (false, $"Booking on {currentDate:yyyy-MM-dd} ({dayOfWeek}) falls outside available hours. Available slots: {availableSlots}");
                }

                currentDate = currentDate.AddDays(1);
            }
    
            return (true, string.Empty);
        }
        #endregion
        //public async Task<APIResponse<object>> GetAvailableEventCentres(DateTime bookedFrom, DateTime bookedTo)
        //{
        //    // Get IDs of event centres that are booked during the specified time range. Only consider active bookings (exclude Cancelled and Rejected)
        //    var bookedEventCentreIds = await _dbContext.Bookings
        //        .Where(b => b.BookingStatus != BookingStatus.Cancelled
        //                 && b.BookingStatus != BookingStatus.Rejected
        //                 && b.BookedFrom < bookedTo
        //                 && b.BookedTo > bookedFrom)
        //        .Select(b => b.EventCentreId)
        //        .Distinct()
        //        .ToListAsync();
        //    var availableEventCentres = await _dbContext.EventCentres
        //        .Where(ec => ec.IsActive && !bookedEventCentreIds.Contains(ec.Id))
        //        .Select(ec => new EventCentreDto
        //        {
        //            Id = ec.Id,
        //            Name = ec.Name,
        //            Location = ec.Location,
        //            Capacity = ec.Capacity,
        //            Description = ec.Description
        //        })
        //        .ToListAsync();
        //    if (availableEventCentres.Count == 0)
        //        return APIResponse<object>.Create(HttpStatusCode.NotFound, "No available event centres found for the specified date and time", null, new Error { Message = "No available event centres found for the specified date and time" });
        //    return APIResponse<object>.Create(HttpStatusCode.OK, "Request successful", availableEventCentres);
        //}

    }
}
