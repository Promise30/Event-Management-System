using AutoMapper;
using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.Event;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;
using Event_Management_System.API.Infrastructures;
using Event_Management_System.API.Infrastructures.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Event_Management_System.API.Application.Implementation
{
    public class EventService : IEventService
    {
        private readonly ILogger<EventService> _logger;
        private readonly IDatabaseRepository<Event, Guid> _databaseRepository;
        private IMapper _mapper;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventService(ApplicationDbContext dbContext, IMapper mapper, ILogger<EventService> logger, UserManager<ApplicationUser> userManager, IDatabaseRepository<Event, Guid> databaseRepository)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
            _databaseRepository = databaseRepository;
        }
        // Implement event-related methods here
        public async Task<APIResponse<EventDto>> CreateEventAsync(Guid userId, CreateEventDto createEventDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) 
                return APIResponse<EventDto>.Create(HttpStatusCode.NotFound, "User does not exist", null);
            
            // Validate EndTime is after StartTime
            if (createEventDto.EndTime.HasValue && createEventDto.EndTime.Value <= createEventDto.StartTime)
                return APIResponse<EventDto>.Create(HttpStatusCode.BadRequest, "End time must be after start time", null);
            
            // Validate StartTime is in the future
            if (createEventDto.StartTime <= DateTime.UtcNow)
                return APIResponse<EventDto>.Create(HttpStatusCode.BadRequest, "Start time must be in the future", null);
            
            var booking = await _dbContext.Bookings
                .Include(b => b.EventCentre)
                .FirstOrDefaultAsync(b => b.Id == createEventDto.BookingId);
            
            if (booking == null)
                return APIResponse<EventDto>.Create(HttpStatusCode.NotFound, "Booking does not exist", null);
            
            // Validate booking belongs to the user
            if (booking.OrganizerId != userId)
                return APIResponse<EventDto>.Create(HttpStatusCode.Forbidden, "You are not authorized to create events for this booking", null);
            
            // Validate booking status
            if (booking.BookingStatus != BookingStatus.Confirmed)
                return APIResponse<EventDto>.Create(HttpStatusCode.BadRequest, 
                    $"Cannot create event. Booking status is '{booking.BookingStatus.GetEnumDescription()}'. Only confirmed bookings can have events.", null);
            
            // Validate event dates are within booking dates
            if (createEventDto.StartTime.Date < booking.BookedFrom.Date || 
                createEventDto.EndTime.HasValue && createEventDto.EndTime.Value.Date > booking.BookedTo.Date)
                return APIResponse<EventDto>.Create(HttpStatusCode.BadRequest, 
                    $"Event dates must be within the booking period ({booking.BookedFrom:yyyy-MM-dd} to {booking.BookedTo:yyyy-MM-dd})", null);
            
            // Validate number of attendees doesn't exceed venue capacity
            if (createEventDto.NumberOfAttendees > booking.EventCentre.Capacity)
                return APIResponse<EventDto>.Create(HttpStatusCode.BadRequest, 
                    $"Number of attendees ({createEventDto.NumberOfAttendees}) exceeds venue capacity ({booking.EventCentre.Capacity})", null);

            var createEvent = new Event
            {
                BookingId = createEventDto.BookingId,
                Title = createEventDto.Title,
                Description = createEventDto.Description,
                StartTime = createEventDto.StartTime,
                EndTime = createEventDto.EndTime,
                EventFlyer = createEventDto.EventFlyer,
                NumberOfAttendees = createEventDto.NumberOfAttendees,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.Events.Add(createEvent);
            var auditLog = new AuditLog
            {
                UserId = userId,
                ObjectId = createEvent.Id,
                Description = $"Added new event with id {createEvent.Id} by {user.Email} at {DateTimeOffset.UtcNow}",
                ActionType = ActionType.Create,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow,
            };
            _dbContext.AuditLogs.Add(auditLog); 
            await _dbContext.SaveChangesAsync();

            var eventDto = _mapper.Map<EventDto>(createEvent);
            return APIResponse<EventDto>.Create(HttpStatusCode.Created, "Event created successfully", eventDto);
        }
        public async Task<APIResponse<EventDto>> GetEventById(Guid eventId)
        {
            var existingEvent = await _dbContext.Events
                .AsNoTracking()
                .Include(e => e.Booking)
                .FirstOrDefaultAsync(e => e.Id == eventId);
            if (existingEvent == null)
                return APIResponse<EventDto>.Create(HttpStatusCode.NotFound, "Event does not exist", null);
            var eventDto = _mapper.Map<EventDto>(existingEvent);
            return APIResponse<EventDto>.Create(HttpStatusCode.OK, "Event retrieved successfully", eventDto);
        }
        public async Task<APIResponse<PagedList<EventDto>>> GetAllEvents(RequestParameters requestParameters)
        {
            var query =  _dbContext.Events
                .AsNoTracking()
                .Include(e => e.Booking)
                .OrderByDescending(e => e.CreatedDate)  
                .AsQueryable();
            var pagedEvents = await _databaseRepository.GetAllPaginatedAsync(query, requestParameters);
            if (!pagedEvents.Data.Any())
            {
                return APIResponse<PagedList<EventDto>>.Create(HttpStatusCode.OK, "No events found",
                    new PagedList<EventDto>(new List<EventDto>(), 0, requestParameters.PageNumber, requestParameters.PageSize));
            }

        var eventDtos = _mapper.Map<List<EventDto>>(pagedEvents.Data);
            var response = new PagedList<EventDto>(eventDtos, pagedEvents.MetaData.TotalCount,
                pagedEvents.MetaData.CurrentPage, pagedEvents.MetaData.PageSize);

            return APIResponse<PagedList<EventDto>>.Create(HttpStatusCode.OK, "Events retrieved successfully", response);
        }
        public async Task<APIResponse<EventDto>> UpdateEventAsync(Guid userId, Guid eventId, UpdateEventDto updateEventDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<EventDto>.Create(HttpStatusCode.NotFound, "User does not exist", null);
            
            // Validate EndTime is after StartTime
            if (updateEventDto.EndTime.HasValue && updateEventDto.EndTime.Value <= updateEventDto.StartTime)
                return APIResponse<EventDto>.Create(HttpStatusCode.BadRequest, "End time must be after start time", null);
            
            var existingEvent = await _dbContext.Events
                .Include(e => e.Booking)
                .ThenInclude(b => b.EventCentre)
                .FirstOrDefaultAsync(e => e.Id == eventId);
            
            if (existingEvent == null)
                return APIResponse<EventDto>.Create(HttpStatusCode.NotFound, "Event does not exist", null);
            
            // Validate user is the organizer
            if (existingEvent.Booking.OrganizerId != userId)
                return APIResponse<EventDto>.Create(HttpStatusCode.Forbidden, "You are not authorized to update this event", null);
            
            // Validate event dates are within booking dates
            if (updateEventDto.StartTime.Date < existingEvent.Booking.BookedFrom.Date || 
                updateEventDto.EndTime.HasValue && updateEventDto.EndTime.Value.Date > existingEvent.Booking.BookedTo.Date)
                return APIResponse<EventDto>.Create(HttpStatusCode.BadRequest, 
                    $"Event dates must be within the booking period ({existingEvent.Booking.BookedFrom:yyyy-MM-dd} to {existingEvent.Booking.BookedTo:yyyy-MM-dd})", null);
            
            // Validate number of attendees doesn't exceed venue capacity
            if (updateEventDto.NumberOfAttendees > existingEvent.Booking.EventCentre.Capacity)
                return APIResponse<EventDto>.Create(HttpStatusCode.BadRequest, 
                    $"Number of attendees ({updateEventDto.NumberOfAttendees}) exceeds venue capacity ({existingEvent.Booking.EventCentre.Capacity})", null);
            
            existingEvent.Title = updateEventDto.Title;
            existingEvent.Description = updateEventDto.Description;
            existingEvent.StartTime = updateEventDto.StartTime;
            existingEvent.EndTime = updateEventDto.EndTime;
            existingEvent.EventFlyer = updateEventDto.EventFlyer;
            existingEvent.NumberOfAttendees = updateEventDto.NumberOfAttendees;
            existingEvent.ModifiedDate = DateTimeOffset.UtcNow;

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    ObjectId = existingEvent.Id,
                    Description = $"Updated event with id {existingEvent.Id} by {user.Email} at {DateTimeOffset.UtcNow}",
                    ActionType = ActionType.Update,
                    CreatedDate = DateTimeOffset.UtcNow,
                    ModifiedDate = DateTimeOffset.UtcNow,
                };
                _dbContext.AuditLogs.Add(auditLog);
                _dbContext.Events.Update(existingEvent);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the event with id {EventId}", eventId);
                await transaction.RollbackAsync();
                return APIResponse<EventDto>.Create(HttpStatusCode.InternalServerError, "An error occurred while updating the event", null);
            }
            var eventDto = _mapper.Map<EventDto>(existingEvent);
            return APIResponse<EventDto>.Create(HttpStatusCode.OK, "Event updated successfully", eventDto);
        }
        public async Task<APIResponse<object>> DeleteEventAsync(Guid userId, Guid eventId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User does not exist", Enumerable.Empty<object>());
            var existingEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (existingEvent == null)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Event does not exist", Enumerable.Empty<object>());

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                _dbContext.Events.Remove(existingEvent);
            var auditLog = new AuditLog
            {
                UserId = userId,
                ObjectId = existingEvent.Id,
                Description = $"Deleted event with id {existingEvent.Id} by {user.Email} at {DateTimeOffset.UtcNow}",
                ActionType = ActionType.Delete,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow,
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the event with id {EventId}", eventId);
                await transaction.RollbackAsync();
                return APIResponse<object>.Create(HttpStatusCode.InternalServerError, "An error occurred while deleting the event", Enumerable.Empty<object>());
            }
            return APIResponse<object>.Create(HttpStatusCode.OK, "Event deleted successfully", Enumerable.Empty<object>());
        }

    }
}
