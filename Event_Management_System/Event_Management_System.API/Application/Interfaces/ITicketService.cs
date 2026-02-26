using Event_Management_System.API.Domain.DTOs.Ticket;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application.Interfaces
{
    public interface ITicketService
    {
        Task<APIResponse<TicketDto>> GetTicketByIdAsync(Guid ticketId);
        Task<APIResponse<PagedResponse<List<TicketDto>>>> GetAllTicketsAsync(RequestParameters requestParameters, SortParameters sortParameters, string? searchTerm, Guid eventId);
        Task<APIResponse<List<TicketDto>>> GetTicketsByAttendeeIdAsync(Guid attendeeId);
        Task<APIResponse<object>> CancelTicketAsync(Guid userId, Guid ticketId);
        Task<APIResponse<CreateTicketResponseDto>> CreateTicketAsync(Guid userId, CreateTicketDto createTicketDto);
    }
}
