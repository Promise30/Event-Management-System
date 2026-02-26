using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.Event;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management_System.API.Controllers
{
    /// <summary>
    /// Manages event operations including creation, retrieval, update, and deletion
    /// </summary>
    [Authorize]
    [Route("events")]
    [ApiController]
    public class EventController : BaseController
    {
        private readonly IEventService _eventService;
        public EventController(IHttpContextAccessor contextAccessor, IConfiguration configuration, IEventService eventService) : base(contextAccessor, configuration)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Retrieve a specific event by its unique identifier
        /// </summary>
        /// <param name="eventId">The unique identifier of the event</param>
        /// <returns>The event details</returns>
        /// <response code="200">Event retrieved successfully</response>
        /// <response code="404">Event not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("get-by-id")]
        [ProducesResponseType(typeof(APIResponse<EventDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEventById(Guid eventId)
        {
            var response = await _eventService.GetEventById(eventId);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Retrieve all events with pagination support
        /// </summary>
        /// <param name="requestParameters">Pagination parameters including page number and page size</param>
        /// <returns>A paginated list of events</returns>
        /// <response code="200">Events retrieved successfully</response>
        /// <response code="404">No events found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("get-all")]
        [ProducesResponseType(typeof(APIResponse<PagedResponse<List<EventDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllEvents([FromQuery] RequestParameters requestParameters)
        {
            var response = await _eventService.GetAllEvents(requestParameters);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Create a new event linked to a booking
        /// </summary>
        /// <param name="createEventDto">The event details including title, description, start and end times</param>
        /// <returns>The newly created event</returns>
        /// <response code="201">Event created successfully</response>
        /// <response code="400">Invalid event data or booking not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("create-event")]
        [Authorize(Roles = "Organizer,Administrator")]
        [ProducesResponseType(typeof(APIResponse<EventDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto createEventDto)
        {
            var userId = GetUserId();
            var response = await _eventService.CreateEventAsync(userId, createEventDto);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Update an existing event's details
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to update</param>
        /// <param name="updateEventDto">The updated event details</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Event updated successfully</response>
        /// <response code="400">Invalid update data</response>
        /// <response code="404">Event not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPut("update-event")]
        [Authorize(Roles = "Organizer,Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEvent([FromQuery] Guid eventId, [FromBody] UpdateEventDto updateEventDto)
        {
            var userId = GetUserId();
            var response = await _eventService.UpdateEventAsync(userId, eventId, updateEventDto);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Delete an event by its unique identifier
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to delete</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Event deleted successfully</response>
        /// <response code="404">Event not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpDelete("delete-event")]
        [Authorize(Roles = "Organizer,Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEvent([FromQuery] Guid eventId)
        {
            var userId = GetUserId();
            var response = await _eventService.DeleteEventAsync(userId, eventId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
