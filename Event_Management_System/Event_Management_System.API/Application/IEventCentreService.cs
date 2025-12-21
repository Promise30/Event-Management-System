using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application
{
    public interface IEventCentreService
    {
        Task<APIResponse<object>> AddEventCentre(Guid userId, AddEventCentreDto addeventCentre);
        Task<APIResponse<object>> AddEventCentreAvailability(Guid userId, AddEventCentreAvailabilityDto availabilityDto);
        Task<APIResponse<object>> DeleteEventCentre(Guid userId, Guid eventCentreId);
        Task<APIResponse<object>> DeleteEventCentreAvailability(Guid userId, Guid availabilityId);
        Task<APIResponse<object>> ReactivateEventCentre(Guid userId, Guid eventCentreId);
        Task<APIResponse<PagedList<EventCentreDto>>> GetAllEventCentresAsync(RequestParameters requestParameters);
        Task<APIResponse<EventCentreDto>> GetEventCentreById(Guid eventCentreId);
        Task<APIResponse<object>> UpdateEventCentreAsync(Guid userId, Guid eventCentreId, AddEventCentreDto eventCentreDto);
        Task<APIResponse<object>> UpdateEventCentreAvailability(Guid userId, Guid availabilityId, UpdateEventCentreAvailabilityDto availabilityDto);
    }
}
