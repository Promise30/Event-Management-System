using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.TicketType;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management_System.API.Controllers
{
    /// <summary>
    /// Manages ticket type configurations for events including creation, retrieval, update, and deletion
    /// </summary>
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

        /// <summary>
        /// Retrieve all ticket types for a specific event with pagination, sorting, and search support
        /// </summary>
        /// <param name="requestParameters">Pagination parameters including page number and page size</param>
        /// <param name="sortParameters">Optional sorting parameters</param>
        /// <param name="searchTerm">Optional search term to filter ticket types by name</param>
        /// <param name="eventId">The unique identifier of the event</param>
        /// <returns>A paginated list of ticket types</returns>
        /// <response code="200">Ticket types retrieved successfully</response>
        /// <response code="404">Event not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("get-ticket-types")]
        [ProducesResponseType(typeof(APIResponse<PagedResponse<List<TicketTypeDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicketTypes([FromQuery] RequestParameters requestParameters, [FromQuery] SortParameters? sortParameters, string? searchTerm, Guid eventId)
        {
            var response = await _ticketTypeService.GetAllTicketTypesAsync(requestParameters, sortParameters, searchTerm, eventId);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Create a new ticket type for an event (e.g., VIP, Regular, Early Bird)
        /// </summary>
        /// <param name="createTicketTypeDto">The ticket type details including name, price, capacity, and event ID</param>
        /// <returns>The newly created ticket type</returns>
        /// <response code="201">Ticket type created successfully</response>
        /// <response code="400">Invalid ticket type data, duplicate name, or event not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("create-ticket-type")]
        [ProducesResponseType(typeof(APIResponse<TicketTypeDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTicketType([FromBody] CreateTicketTypeDto createTicketTypeDto)
        {
            var userId = GetUserId();
            var response = await _ticketTypeService.CreateTicketTypeAsync(userId, createTicketTypeDto);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Update an existing ticket type's details
        /// </summary>
        /// <param name="ticketTypeId">The unique identifier of the ticket type to update</param>
        /// <param name="createTicketTypeDto">The updated ticket type details</param>
        /// <returns>The updated ticket type</returns>
        /// <response code="200">Ticket type updated successfully</response>
        /// <response code="400">Invalid update data</response>
        /// <response code="404">Ticket type not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPut("update-ticket-type")]
        [ProducesResponseType(typeof(APIResponse<TicketTypeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTicketType([FromQuery] Guid ticketTypeId, [FromBody] CreateTicketTypeDto createTicketTypeDto)
        {
            var userId = GetUserId();
            var response = await _ticketTypeService.UpdateTicketTypeAsync(userId, ticketTypeId, createTicketTypeDto);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Delete a ticket type by its unique identifier
        /// </summary>
        /// <param name="ticketTypeId">The unique identifier of the ticket type to delete</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Ticket type deleted successfully</response>
        /// <response code="400">Ticket type has issued tickets and cannot be deleted</response>
        /// <response code="404">Ticket type not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpDelete("delete-ticket-type")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTicketType([FromQuery] Guid ticketTypeId)
        {
            var userId = GetUserId();
            var response = await _ticketTypeService.DeleteTicketTypeAsync(userId, ticketTypeId);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Retrieve a specific ticket type by its unique identifier
        /// </summary>
        /// <param name="ticketTypeId">The unique identifier of the ticket type</param>
        /// <returns>The ticket type details</returns>
        /// <response code="200">Ticket type retrieved successfully</response>
        /// <response code="404">Ticket type not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("get-ticket-type-by-id")]
        [ProducesResponseType(typeof(APIResponse<TicketTypeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicketTypeById([FromQuery] Guid ticketTypeId)
        {
            var response = await _ticketTypeService.GetTicketTypeByIdAsync(ticketTypeId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
