using AutoMapper;
using Azure;
using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.EventCenter;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;
using Event_Management_System.API.Infrastructures;
using Event_Management_System.API.Infrastructures.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Event_Management_System.API.Application.Implementation
{
    public class EventCentreService : IEventCentreService
    {
        private readonly ILogger<EventCentreService> _logger;
        private readonly IDatabaseRepository<EventCentre, Guid> _databaseRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public EventCentreService(ILogger<EventCentreService> logger, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IMapper mapper, IDatabaseRepository<EventCentre, Guid> databaseRepository)
        {
            _logger = logger;
            _dbContext = dbContext;
            _userManager = userManager;
            _mapper = mapper;
            _databaseRepository = databaseRepository;
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
                PricePerDay = addeventCentre.PricePerDay,
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

            var paginatedEventCenters = await _databaseRepository.GetAllPaginatedAsync(query, requestParameters);

            if (paginatedEventCenters.Data == null || paginatedEventCenters.Data.Count == 0)
                return APIResponse<PagedList<EventCentreDto>>.Create(HttpStatusCode.OK, "No event centres found",
                    new PagedList<EventCentreDto>(new List<EventCentreDto>(), 0, requestParameters.PageNumber, requestParameters.PageSize));

            // Map the event centre entities to DTOs
            var eventCentreDtos = _mapper.Map<List<EventCentreDto>>(paginatedEventCenters.Data);

            // Create a new PagedList with the DTOs but preserve the metadata
            var response = new PagedList<EventCentreDto>(eventCentreDtos, paginatedEventCenters.MetaData.TotalCount,
                paginatedEventCenters.MetaData.CurrentPage, paginatedEventCenters.MetaData.PageSize);

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
            eventCentre.PricePerDay = eventCentreDto.PricePerDay;
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
        public async Task<APIResponse<object>> AddEventCentreAvailability(Guid userId, List<AddEventCentreAvailabilityDto> availabilityDtos)
        {
            // Validate input
            if (availabilityDtos == null || !availabilityDtos.Any())
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "At least one availability must be provided", null, new Error { Message = "Availability list cannot be empty" });

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User does not exist", null, new Error { Message = "User does not exist" });

            // Get the event centre ID from the first item (they should all be for the same event centre)
            var eventCentreId = availabilityDtos.First().EventCentreId;

            // Validate all items are for the same event centre
            if (availabilityDtos.Any(dto => dto.EventCentreId != eventCentreId))
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "All availabilities must be for the same event centre", null, new Error { Message = "Inconsistent event centre IDs in request" });

            var eventCentre = await _dbContext.EventCentres.Include(ec => ec.Availabilities).FirstOrDefaultAsync(ec => ec.Id == eventCentreId);
            if (eventCentre == null)
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Event centre does not exist", null, new Error { Message = "Event centre does not exist" });

            // Validate each availability DTO for logical errors
            foreach (var dto in availabilityDtos)
            {
                if (dto.CloseTime <= dto.OpenTime)
                    return APIResponse<object>.Create(HttpStatusCode.BadRequest, $"Close time must be after open time for {dto.Day}", null, new Error { Message = "Invalid time range" });
            }

            // Check for duplicates within the new list itself
            var duplicatesInList = availabilityDtos
                .GroupBy(dto => new { dto.Day, dto.OpenTime, dto.CloseTime })
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatesInList.Any())
            {
                var duplicateInfo = string.Join(", ", duplicatesInList.Select(d => $"{d.Day} {d.OpenTime}-{d.CloseTime}"));
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, $"Duplicate availabilities found in the request: {duplicateInfo}", null, new Error { Message = "Duplicate availabilities in request" });
            }

            // Check each new availability against existing ones
            var validationErrors = new List<string>();
            
            foreach (var dto in availabilityDtos)
            {
                // Check if same day/time slot already exists in database
                bool isDuplicate = eventCentre.Availabilities
                    .Any(a => a.Day == dto.Day
                            && a.OpenTime == dto.OpenTime
                            && a.CloseTime == dto.CloseTime);

                if (isDuplicate)
                {
                    validationErrors.Add($"Availability for {dto.Day} from {dto.OpenTime:hh\\:mm} to {dto.CloseTime:hh\\:mm} already exists");
                    continue;
                }

                // Check for overlapping availability with existing records
                bool isOverlapping = eventCentre.Availabilities.Any(a => 
                    a.Day == dto.Day
                    && (dto.OpenTime >= a.OpenTime && dto.OpenTime < a.CloseTime
                    || dto.CloseTime > a.OpenTime && dto.CloseTime <= a.CloseTime
                    || dto.OpenTime <= a.OpenTime && dto.CloseTime >= a.CloseTime));

                if (isOverlapping)
                {
                    var overlappingSlot = eventCentre.Availabilities.First(a => 
                        a.Day == dto.Day
                        && (dto.OpenTime >= a.OpenTime && dto.OpenTime < a.CloseTime
                        || dto.CloseTime > a.OpenTime && dto.CloseTime <= a.CloseTime
                        || dto.OpenTime <= a.OpenTime && dto.CloseTime >= a.CloseTime));

                    validationErrors.Add($"Availability for {dto.Day} from {dto.OpenTime:hh\\:mm} to {dto.CloseTime:hh\\:mm} overlaps with existing slot {overlappingSlot.OpenTime:hh\\:mm}-{overlappingSlot.CloseTime:hh\\:mm}");
                }
            }

            // Check for overlaps within the new list
            for (int i = 0; i < availabilityDtos.Count; i++)
            {
                for (int j = i + 1; j < availabilityDtos.Count; j++)
                {
                    var dto1 = availabilityDtos[i];
                    var dto2 = availabilityDtos[j];

                    if (dto1.Day == dto2.Day)
                    {
                        bool overlaps = dto1.OpenTime >= dto2.OpenTime && dto1.OpenTime < dto2.CloseTime
                            || dto1.CloseTime > dto2.OpenTime && dto1.CloseTime <= dto2.CloseTime
                            || dto1.OpenTime <= dto2.OpenTime && dto1.CloseTime >= dto2.CloseTime;

                        if (overlaps)
                        {
                            validationErrors.Add($"Overlapping availabilities in request for {dto1.Day}: {dto1.OpenTime:hh\\:mm}-{dto1.CloseTime:hh\\:mm} and {dto2.OpenTime:hh\\:mm}-{dto2.CloseTime:hh\\:mm}");
                        }
                    }
                }
            }

            if (validationErrors.Any())
            {
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "Validation failed", null, new Error { Message = string.Join("; ", validationErrors) });
            }

            // All validations passed, add the availabilities
            var newAvailabilities = new List<EventCentreAvailability>();
            
            foreach (var dto in availabilityDtos)
            {
                var newAvailability = new EventCentreAvailability
                {
                    EventCentreId = dto.EventCentreId,
                    OpenTime = dto.OpenTime,
                    Day = dto.Day,
                    CloseTime = dto.CloseTime,
                    CreatedDate = DateTimeOffset.UtcNow,
                    ModifiedDate = DateTimeOffset.UtcNow
                };
                newAvailabilities.Add(newAvailability);
                _dbContext.Availabilities.Add(newAvailability);
            }

            // Create a single comprehensive audit log
            var availabilityDetails = string.Join(", ", newAvailabilities.Select(a => $"{a.Day} ({a.OpenTime:hh\\:mm}-{a.CloseTime:hh\\:mm})"));
            var auditLog = new AuditLog
            {
                ObjectId = eventCentre.Id,
                ActionType = ActionType.Create,
                UserId = user.Id,
                Description = $"Added {newAvailabilities.Count} new availability slot(s) for event center {eventCentre.Name}: {availabilityDetails}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            
            await _dbContext.SaveChangesAsync();
            
            var response = newAvailabilities.Select(a => _mapper.Map<EventCentreAvailabilityDto>(a)).ToList();
            return APIResponse<object>.Create(HttpStatusCode.Created, $"Successfully added {newAvailabilities.Count} availability slot(s)", response);
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
                   && (availabilityDto.OpenTime >= a.OpenTime && availabilityDto.OpenTime < a.CloseTime
                    || availabilityDto.CloseTime > a.OpenTime && availabilityDto.CloseTime <= a.CloseTime
                    || availabilityDto.OpenTime <= a.OpenTime && availabilityDto.CloseTime >= a.CloseTime));

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
