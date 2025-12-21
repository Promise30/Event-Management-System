using Event_Management_System.API.Application;
using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management_System.API.Controllers
{
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
        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetTicketById(Guid ticketId)
        {
            var response = await _ticketService.GetTicketByIdAsync(ticketId);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllTickets([FromQuery] RequestParameters requestParameters, [FromQuery] SortParameters sortParameters, string? searchTerm, Guid eventId)
        {
            var response = await _ticketService.GetAllTicketsAsync(requestParameters, sortParameters, searchTerm, eventId);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpGet("get-by-attendee")]
        public async Task<IActionResult> GetTicketsByAttendeeId(Guid attendeeId)
        {
            var response = await _ticketService.GetTicketsByAttendeeIdAsync(attendeeId);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPost("create-ticket")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto createTicketDto)
        {
            var userId = GetUserId();
            var response = await _ticketService.CreateTicketAsync(userId, createTicketDto);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpDelete("cancel-ticket")]
        public async Task<IActionResult> CancelTicket(Guid ticketId)
        {
            var userId = GetUserId();
            var response = await _ticketService.CancelTicketAsync(userId, ticketId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
