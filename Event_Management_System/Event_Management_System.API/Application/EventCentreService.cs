using AutoMapper;
using Azure;
using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;
using Event_Management_System.API.Infrastructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Event_Management_System.API.Application
{
    public class EventCentreService : IEventCentreService
    {
        public readonly ILogger<EventCentreService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public EventCentreService(ILogger<EventCentreService> logger, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _logger = logger;
            _dbContext = dbContext;
            _userManager = userManager;
            _mapper = mapper;
        }

        // Add Event Centre
        public async Task<APIResponse<object>> AddEventCentre(Guid userId, AddEventCentreDto addeventCentre)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User does not exist", null, new Error { Message = "User does not exist" });

            var eventCentreToCreate = new EventCentre
            {
                Name = addeventCentre.Name,
                Description = addeventCentre.Description,
                Location = addeventCentre.Location,
                Capacity = addeventCentre.Capacity,
                IsActive = true,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow,
            };
            _dbContext.EventCentres.Add(eventCentreToCreate);

            var auditLog = new AuditLog
            {
                ObjectId = eventCentreToCreate.Id,
                ActionType = ActionType.Create,
                UserId = user.Id,
                Description = $"Created new event center {eventCentreToCreate.Name} by organizer {user.Id} at {DateTimeOffset.UtcNow}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();

            var response = _mapper.Map<EventCentreDto>(eventCentreToCreate);
            return APIResponse<object>.Create(HttpStatusCode.Created, "Request successful", response);

        }
        public async Task<APIResponse<EventCentreDto>> GetEventCentreById(Guid eventCentreId)
        {
            var eventCentre = await _dbContext.EventCentres
                .Include(ec => ec.Availabilities)
                .Include(e => e.Bookings)
                    .ThenInclude(e => e.Event)
                    .AsSplitQuery()
                .FirstOrDefaultAsync(e => e.Id == eventCentreId);
            if (eventCentre == null)
                return APIResponse<EventCentreDto>.Create(HttpStatusCode.NotFound, "Event center does not exist", null, new Error { Message = "Event center does not exist" });
            var result = _mapper.Map<EventCentreDto>(eventCentre);
            return APIResponse<EventCentreDto>.Create(HttpStatusCode.OK, "Request successful", result);
        }
        public async Task<APIResponse<PagedList<EventCentreDto>>> GetAllEventCentresAsync(RequestParameters requestParameters)
        {
            IQueryable<EventCentre> query = _dbContext.EventCentres
                .Include(ec => ec.Availabilities)
                .Where(e => e.IsActive == true)
                .OrderByDescending(ec => ec.CreatedDate);

            // Use PagedList.ToPagedList method to apply pagination at the database level
            var pagedEventCentres = await PagedList<EventCentre>.ToPagedList(query, null, requestParameters);

            if (pagedEventCentres.Data == null || pagedEventCentres.Data.Count == 0)
                return APIResponse<PagedList<EventCentreDto>>.Create(HttpStatusCode.OK, "No event centres found",
                    new PagedList<EventCentreDto>(new List<EventCentreDto>(), 0, requestParameters.PageNumber, requestParameters.PageSize));

            // Map the event centre entities to DTOs
            var eventCentreDtos = _mapper.Map<List<EventCentreDto>>(pagedEventCentres.Data);

            // Create a new PagedList with the DTOs but preserve the metadata
            var response = new PagedList<EventCentreDto>(eventCentreDtos, pagedEventCentres.MetaData.TotalCount,
                pagedEventCentres.MetaData.CurrentPage, pagedEventCentres.MetaData.PageSize);

            return APIResponse<PagedList<EventCentreDto>>.Create(HttpStatusCode.OK, "Request successful", response);
        }

        public async Task<APIResponse<object>> DeleteEventCentre(Guid userId, Guid eventCentreId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User does not exist", null);
            var eventCentre = await _dbContext.EventCentres.FirstOrDefaultAsync(e => e.Id.Equals(eventCentreId));
            if (eventCentre == null)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Event centre does not exist", null, new Error { Message = "Event centre does not exist" });
            eventCentre.IsActive = false;
            eventCentre.ModifiedDate = DateTimeOffset.UtcNow;

            _dbContext.EventCentres.Update(eventCentre);

            var auditLog = new AuditLog
            {
                UserId = userId,
                ObjectId = eventCentreId,
                ActionType = ActionType.Delete,
                Description = $"Updated event centre '{eventCentre.Name}' status to Inactive by user '{user.Id}' at {DateTimeOffset.UtcNow}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
            return APIResponse<object>.Create(HttpStatusCode.NoContent, "Request Successful", null);
        }

        public async Task<APIResponse<object>> UpdateEventCentreAsync(Guid userId, Guid eventCentreId, AddEventCentreDto eventCentreDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User does not exist", null);
            var eventCentre = await _dbContext.EventCentres.FirstOrDefaultAsync(e => e.Id == eventCentreId);
            if (eventCentre == null)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Event centre does not exist", null, new Error { Message = "Event centre does not exist" });

            eventCentre.Name = eventCentreDto.Name;
            eventCentre.Capacity = eventCentreDto.Capacity;
            eventCentre.Description = eventCentreDto.Description;
            eventCentre.Location = eventCentreDto.Location;
            eventCentre.ModifiedDate = DateTimeOffset.UtcNow;

            _dbContext.EventCentres.Update(eventCentre);

            var auditLog = new AuditLog
            {
                UserId = userId,
                ObjectId = eventCentreId,
                ActionType = ActionType.Update,
                Description = $"Updated event centre '{eventCentre.Name}' details by user '{user.Id}' at {DateTimeOffset.UtcNow}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
            return APIResponse<object>.Create(HttpStatusCode.NoContent, "Request Successful", null);
        }
        public async Task<APIResponse<object>> ReactivateEventCentre(Guid userId, Guid eventCentreId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User does not exist", null);
            var eventCentre = await _dbContext.EventCentres.FirstOrDefaultAsync(e => e.Id == eventCentreId);
            if (eventCentre == null)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Event center does not exist", null, new Error { Message = "Event centre does not exist" });

            eventCentre.IsActive = true;
            eventCentre.ModifiedDate = DateTimeOffset.UtcNow;

            _dbContext.EventCentres.Update(eventCentre);

            var auditLog = new AuditLog
            {
                UserId = userId,
                ObjectId = eventCentreId,
                ActionType = ActionType.Update,
                Description = $"Updated event center status to become available '{eventCentre.Name}' by user '{user.Id}' at {DateTimeOffset.UtcNow}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
            return APIResponse<object>.Create(HttpStatusCode.NoContent, "Request Successful", null);
        }

        // Event Centre Availability Management 
        public async Task<APIResponse<object>> AddEventCentreAvailability(Guid userId, AddEventCentreAvailabilityDto availabilityDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User does not exist", null, new Error { Message = "User does not exist" });
            var eventCentre = await _dbContext.EventCentres.Include(ec => ec.Availabilities).FirstOrDefaultAsync(ec => ec.Id == availabilityDto.EventCentreId);
            if (eventCentre == null)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Event centre does not exist", null, new Error { Message = "Event centre does not exist" });

            // check if same day/time slot already exists
            bool isDuplicate = eventCentre.Availabilities
                .Any(a => a.EventCentreId == availabilityDto.EventCentreId
                        && a.Day == availabilityDto.Day
                        && a.OpenTime == availabilityDto.OpenTime
                        && a.CloseTime == availabilityDto.CloseTime);
            if (isDuplicate)
            {
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "The specified availability already exists.", null, new Error { Message = "The specified availability already exists." });
            }
            // Check for overlapping availability
            bool isOverlapping = eventCentre.Availabilities.Any(a => a.EventCentreId == availabilityDto.EventCentreId
                && a.Day == availabilityDto.Day
                && ((availabilityDto.OpenTime >= a.OpenTime && availabilityDto.OpenTime < a.CloseTime)
                || (availabilityDto.CloseTime > a.OpenTime && availabilityDto.CloseTime <= a.CloseTime)
                || (availabilityDto.OpenTime <= a.OpenTime && availabilityDto.CloseTime >= a.CloseTime)
                ));

            if (isOverlapping)
            {
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "The specified availability overlaps with an existing availability.", null, new Error { Message = "The specified availability overlaps with an existing availability." });
            }
            var newAvailability = new EventCentreAvailability
            {
                EventCentreId = availabilityDto.EventCentreId,
                OpenTime = availabilityDto.OpenTime,
                Day = availabilityDto.Day,
                CloseTime = availabilityDto.CloseTime,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
             _dbContext.Availabilities.Add(newAvailability);
            var auditLog = new AuditLog
            {
                ObjectId = eventCentre.Id,
                ActionType = ActionType.Create,
                UserId = user.Id,
                Description = $"Added new availability for event center {eventCentre.Name} from {newAvailability.OpenTime} to {newAvailability.CloseTime}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
            var response = _mapper.Map<EventCentreAvailabilityDto>(newAvailability);
            return APIResponse<object>.Create(HttpStatusCode.Created, "Request successful", response);
        }

        public async Task<APIResponse<object>> DeleteEventCentreAvailability(Guid userId, Guid availabilityId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User does not exist", null);
            var availability = await _dbContext.Availabilities.Include(a => a.EventCentre).FirstOrDefaultAsync(a => a.Id == availabilityId);
            if (availability == null)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Availability does not exist", null, new Error { Message = "Availability does not exist" });
            
            _dbContext.Availabilities.Remove(availability);
            var auditLog = new AuditLog
            {
                UserId = userId,
                ObjectId = availability.Id,
                ActionType = ActionType.Delete,
                Description = $"Deleted availability for '{availability.EventCentre.Name}' on {availability.Day} from {availability.OpenTime} to {availability.CloseTime} by {user.Id} by {DateTimeOffset.UtcNow}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
            return APIResponse<object>.Create(HttpStatusCode.NoContent, "Request Successful", null);
        }

        public async Task<APIResponse<object>> UpdateEventCentreAvailability(Guid userId, Guid availabilityId, UpdateEventCentreAvailabilityDto availabilityDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User does not exist", null);
            var availability = await _dbContext.Availabilities.FirstOrDefaultAsync(a => a.Id == availabilityId);

            //var availability = await _dbContext.Availabilities.Include(a => a.EventCentre).FirstOrDefaultAsync(a => a.Id == availabilityId);
            //var availability = await _dbContext.Availabilities.Include(a => a.EventCentre).FirstOrDefaultAsync(a => a.Id == availabilityId);

            if (availability == null)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Availability does not exist", null, new Error { Message = "Availability does not exist" });

            if (availabilityDto.CloseTime <= availabilityDto.OpenTime)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "Close time must be after open time", null, new Error { Message = "Close time must be after open time" });

            // Check for duplicate
            bool isDuplicate = await _dbContext.Availabilities
                 .AnyAsync(a => a.EventCentreId == availability.EventCentreId  //  Use the foreign key
                   && a.Id != availabilityId  // Exclude current record
                   && a.Day == availabilityDto.Day
                   && a.OpenTime == availabilityDto.OpenTime
                   && a.CloseTime == availabilityDto.CloseTime);

            if (isDuplicate)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "The specified availability already exists.", null, new Error { Message = "The specified availability already exists." });

            bool isOverlapping = await _dbContext.Availabilities
                   .AnyAsync(a => a.EventCentreId == availability.EventCentreId  // ← Use the foreign key
                   && a.Id != availabilityId  // Exclude current record
                   && a.Day == availabilityDto.Day
                   && ((availabilityDto.OpenTime >= a.OpenTime && availabilityDto.OpenTime < a.CloseTime)
                    || (availabilityDto.CloseTime > a.OpenTime && availabilityDto.CloseTime <= a.CloseTime)
                    || (availabilityDto.OpenTime <= a.OpenTime && availabilityDto.CloseTime >= a.CloseTime)));

            if (isOverlapping)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "The specified availability overlaps with an existing availability.", null, new Error { Message = "The specified availability overlaps with an existing availability." });

            // Update availability
            availability.OpenTime = availabilityDto.OpenTime;
            availability.CloseTime = availabilityDto.CloseTime;
            availability.Day = availabilityDto.Day;
            availability.ModifiedDate = DateTimeOffset.UtcNow;

            _dbContext.Availabilities.Update(availability);
            var auditLog = new AuditLog
            {
                UserId = userId,
                ObjectId = availability.Id,
                ActionType = ActionType.Update,
                Description = $"Updated availability for event centre '{availability.Id}' on {availability.Day} to {availability.OpenTime} - {availability.CloseTime}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
            return APIResponse<object>.Create(HttpStatusCode.NoContent, "Request Successful", null);

        }  
    }
}
