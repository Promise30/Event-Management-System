using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application
{
    public interface ITicketTypeService
    {
        Task<APIResponse<object>> CreateTicketTypeAsync(Guid userId, CreateTicketTypeDto createTicketTypeDto);
        Task<APIResponse<PagedResponse<List<TicketTypeDto>>>> GetAllTicketTypesAsync(RequestParameters requestParameters,SortParameters? sortParameters, string? searchTerm, Guid eventId);
        Task<APIResponse<object>> DeleteTicketTypeAsync(Guid userId, Guid ticketTypeId);
        Task<APIResponse<object>> UpdateTicketTypeAsync(Guid userId, Guid ticketTypeId, CreateTicketTypeDto createTicketTypeDto);
        Task<APIResponse<TicketTypeDto>> GetTicketTypeByIdAsync(Guid ticketTypeId);

      
    }
}
