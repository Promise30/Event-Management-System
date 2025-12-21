using AutoMapper;
using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;
using Event_Management_System.API.Infrastructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Event_Management_System.API.Application
{
    public class EventService : IEventService
    {
        private readonly ILogger<EventService> _logger;
        private IMapper _mapper;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventService(ApplicationDbContext dbContext, IMapper mapper, ILogger<EventService> logger, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }
        // Implement event-related methods here
        public async Task<APIResponse<EventDto>> CreateEventAsync(Guid userId, CreateEventDto createEventDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) 
                return APIResponse<EventDto>.Create(HttpStatusCode.NotFound, "User does not exist", null);
            var booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Equals(createEventDto.BookingId));
            if (booking == null)
                return APIResponse<EventDto>.Create(HttpStatusCode.NotFound, "Booking does not exist", null);

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
                .Include(e => e.Booking)
                .FirstOrDefaultAsync(e => e.Id == eventId);
            if (existingEvent == null)
                return APIResponse<EventDto>.Create(HttpStatusCode.NotFound, "Event does not exist", null);
            var eventDto = _mapper.Map<EventDto>(existingEvent);
            return APIResponse<EventDto>.Create(HttpStatusCode.OK, "Event retrieved successfully", eventDto);
        }
        public async Task<APIResponse<IEnumerable<EventDto>>> GetAllEvents(RequestParameters requestParameters)
        {
            var events = await _dbContext.Events
                .Include(e => e.Booking)
                .ToListAsync();
            if (events.Count <= 0 || events == null)
                return APIResponse<IEnumerable<EventDto>>.Create(HttpStatusCode.OK, "No events found", new List<EventDto>());

            var eventDtos = _mapper.Map<IEnumerable<EventDto>>(events);
            return APIResponse<IEnumerable<EventDto>>.Create(HttpStatusCode.OK, "Events retrieved successfully", eventDtos);
        }
        public async Task<APIResponse<EventDto>> UpdateEventAsync(Guid userId, Guid eventId, UpdateEventDto updateEventDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<EventDto>.Create(HttpStatusCode.NotFound, "User does not exist", null);
            var existingEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (existingEvent == null)
                return APIResponse<EventDto>.Create(HttpStatusCode.NotFound, "Event does not exist", null);
            existingEvent.Title = updateEventDto.Title;
            existingEvent.Description = updateEventDto.Description;
            existingEvent.StartTime = updateEventDto.StartTime;
            existingEvent.EndTime = updateEventDto.EndTime;
            existingEvent.EventFlyer = updateEventDto.EventFlyer;
            existingEvent.NumberOfAttendees = updateEventDto.NumberOfAttnedees;
            existingEvent.ModifiedDate = DateTimeOffset.UtcNow;
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
            await _dbContext.SaveChangesAsync();
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
            return APIResponse<object>.Create(HttpStatusCode.OK, "Event deleted successfully", Enumerable.Empty<object>());
        }

    }
}
