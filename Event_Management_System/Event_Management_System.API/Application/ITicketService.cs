using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application
{
    public interface ITicketService
    {
        Task<APIResponse<TicketDto>> GetTicketByIdAsync(Guid ticketId);
        Task<APIResponse<PagedResponse<List<TicketDto>>>> GetAllTicketsAsync(RequestParameters requestParameters, SortParameters sortParameters, string? searchTerm, Guid eventId);
        Task<APIResponse<List<TicketDto>>> GetTicketsByAttendeeIdAsync(Guid attendeeId);
        Task<APIResponse<object>> CancelTicketAsync(Guid userId, Guid ticketId);
        Task<APIResponse<object>> CreateTicketAsync(Guid userId, CreateTicketDto createTicketDto);
    }
}
