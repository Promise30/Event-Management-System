using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.Payment;
using Event_Management_System.API.Domain.DTOs.Ticket;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management_System.API.Controllers
{
    /// <summary>
    /// Manages ticket operations including creation, retrieval, and cancellation
    /// </summary>
    [Authorize]
    [Route("tickets")]
    [ApiController]
    public class TicketController : BaseController
    {
        private readonly ITicketService _ticketService;
        public TicketController(IHttpContextAccessor contextAccessor, IConfiguration configuration, ITicketService ticketService) : base(contextAccessor, configuration)
        {
            _ticketService = ticketService;
        }

        /// <summary>
        /// Retrieve a specific ticket by its unique identifier
        /// </summary>
        /// <param name="ticketId">The unique identifier of the ticket</param>
        /// <returns>The ticket details</returns>
        /// <response code="200">Ticket retrieved successfully</response>
        /// <response code="404">Ticket not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("get-by-id")]
        [ProducesResponseType(typeof(APIResponse<TicketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicketById(Guid ticketId)
        {
            var response = await _ticketService.GetTicketByIdAsync(ticketId);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Retrieve all tickets for a specific event with pagination, sorting, and search support
        /// </summary>
        /// <param name="requestParameters">Pagination parameters including page number and page size</param>
        /// <param name="sortParameters">Optional sorting parameters (e.g., sort by ticket number, type, or date)</param>
        /// <param name="searchTerm">Optional search term to filter tickets by number or type</param>
        /// <param name="eventId">The unique identifier of the event</param>
        /// <returns>A paginated list of tickets</returns>
        /// <response code="200">Tickets retrieved successfully</response>
        /// <response code="404">Event not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("get-all")]
        [ProducesResponseType(typeof(APIResponse<PagedResponse<List<TicketDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTickets([FromQuery] RequestParameters requestParameters, [FromQuery] SortParameters sortParameters, string? searchTerm, Guid eventId)
        {
            var response = await _ticketService.GetAllTicketsAsync(requestParameters, sortParameters, searchTerm, eventId);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Retrieve all tickets for a specific attendee
        /// </summary>
        /// <param name="attendeeId">The unique identifier of the attendee</param>
        /// <returns>A list of tickets belonging to the attendee</returns>
        /// <response code="200">Tickets retrieved successfully</response>
        /// <response code="404">Attendee not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("get-by-attendee")]
        [ProducesResponseType(typeof(APIResponse<List<TicketDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicketsByAttendeeId(Guid attendeeId)
        {
            var response = await _ticketService.GetTicketsByAttendeeIdAsync(attendeeId);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Create a new ticket for the authenticated user. For paid ticket types, a Paystack payment will be initiated
        /// and the response will include a payment URL for the user to complete payment.
        /// </summary>
        /// <param name="createTicketDto">The ticket creation details including ticket type ID and event ID</param>
        /// <returns>
        /// For free tickets: ticket confirmation details.
        /// For paid tickets: ticket reservation details with Paystack payment URL.
        /// </returns>
        /// <response code="201">Ticket created (free) or reserved with payment URL (paid)</response>
        /// <response code="400">No tickets available, user already has a ticket, or invalid data</response>
        /// <response code="404">User or ticket type not found</response>
        /// <response code="500">An internal server error occurred or payment initialization failed</response>
        [HttpPost("create-ticket")]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto createTicketDto)
        {
            var userId = GetUserId();
            var response = await _ticketService.CreateTicketAsync(userId, createTicketDto);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Cancel an existing ticket for the authenticated user
        /// </summary>
        /// <param name="ticketId">The unique identifier of the ticket to cancel</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Ticket cancelled successfully</response>
        /// <response code="400">Ticket is already cancelled</response>
        /// <response code="404">User or ticket not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpDelete("cancel-ticket")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelTicket(Guid ticketId)
        {
            var userId = GetUserId();
            var response = await _ticketService.CancelTicketAsync(userId, ticketId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
