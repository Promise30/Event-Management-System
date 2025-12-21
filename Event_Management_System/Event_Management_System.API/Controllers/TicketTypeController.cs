using Event_Management_System.API.Application;
using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management_System.API.Controllers
{
    [Authorize]
    [Route("ticket-type")]
    [ApiController]
    public class TicketTypeController : BaseController
    {
        private readonly ITicketTypeService _ticketTypeService;
        public TicketTypeController(IHttpContextAccessor contextAccessor, IConfiguration configuration, ITicketTypeService ticketTypeService) : base(contextAccessor, configuration)
        {
            _ticketTypeService = ticketTypeService;
        }
        [HttpGet("get-ticket-types")]
        public async Task<IActionResult> GetTicketTypes([FromQuery]RequestParameters requestParameters,[FromQuery] SortParameters? sortParameters, string? searchTerm, Guid eventId)
        {
            var response = await _ticketTypeService.GetAllTicketTypesAsync(requestParameters, sortParameters, searchTerm, eventId);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPost("create-ticket-type")]
        public async Task<IActionResult> CreateTicketType([FromBody] CreateTicketTypeDto createTicketTypeDto)
        {
            var userId = GetUserId();
            var response = await _ticketTypeService.CreateTicketTypeAsync(userId, createTicketTypeDto);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPut("update-ticket-type")]
        public async Task<IActionResult> UpdateTicketType([FromQuery] Guid ticketTypeId, [FromBody] CreateTicketTypeDto createTicketTypeDto)
        {
            var userId = GetUserId();
            var response = await _ticketTypeService.UpdateTicketTypeAsync(userId, ticketTypeId, createTicketTypeDto);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpDelete("delete-ticket-type")]
        public async Task<IActionResult> DeleteTicketType([FromQuery] Guid ticketTypeId)
        {
            var userId = GetUserId();
            var response = await _ticketTypeService.DeleteTicketTypeAsync(userId, ticketTypeId);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpGet("get-ticket-type-by-id")]
        public async Task<IActionResult> GetTicketTypeById([FromQuery] Guid ticketTypeId)
        {
            var response = await _ticketTypeService.GetTicketTypeByIdAsync(ticketTypeId);
            return StatusCode((int)response.StatusCode, response);
        }


    }
}
