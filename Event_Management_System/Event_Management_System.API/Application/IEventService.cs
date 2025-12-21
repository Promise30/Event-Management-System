using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application
{
    public interface IEventService
    {
        Task<APIResponse<EventDto>> CreateEventAsync(Guid userId, CreateEventDto createEventDto);
        Task<APIResponse<EventDto>> GetEventById(Guid eventId);
        Task<APIResponse<IEnumerable<EventDto>>> GetAllEvents(RequestParameters requestParameters);
        Task<APIResponse<object>> DeleteEventAsync(Guid userId, Guid eventId);
        Task<APIResponse<EventDto>> UpdateEventAsync(Guid userId, Guid eventId, UpdateEventDto updateEventDto);
    }
}
