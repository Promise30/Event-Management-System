using Event_Management_System.API.Domain.DTOs.Event;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application.Interfaces
{
    public interface IEventService
    {
        Task<APIResponse<EventDto>> CreateEventAsync(Guid userId, CreateEventDto createEventDto);
        Task<APIResponse<EventDto>> GetEventById(Guid eventId);
        Task<APIResponse<PagedList<EventDto>>> GetAllEvents(RequestParameters requestParameters);
        Task<APIResponse<object>> DeleteEventAsync(Guid userId, Guid eventId);
        Task<APIResponse<EventDto>> UpdateEventAsync(Guid userId, Guid eventId, UpdateEventDto updateEventDto);
    }
}
