using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.EventCenter;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Event_Management_System.API.Controllers
{
    /// <summary>
    /// Manages event centre operations including CRUD and availability scheduling
    /// </summary>
    [Authorize(Roles = "Administrator,Organizer")]
    [Authorize]
    [Route("event-centers")]
    [ApiController]
    public class EventCentreController : BaseController
    {
        private readonly IEventCentreService _eventCentreService;
        public EventCentreController(IHttpContextAccessor contextAccessor, IConfiguration configuration, IEventCentreService eventCentreService) : base(contextAccessor, configuration)
        {
            _eventCentreService = eventCentreService;
        }

        /// <summary>
        /// Retrieve all event centres with pagination support
        /// </summary>
        /// <param name="requestParameters">Pagination parameters including page number and page size</param>
        /// <returns>A paginated list of event centres</returns>
        /// <response code="200">Event centres retrieved successfully</response>
        /// <response code="404">No event centres found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("get-all")]
        [ProducesResponseType(typeof(APIResponse<PagedResponse<List<EventCentreDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] RequestParameters requestParameters)
        {
            var result = await _eventCentreService.GetAllEventCentresAsync(requestParameters);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Retrieve a specific event centre by its unique identifier
        /// </summary>
        /// <param name="eventCenterId">The unique identifier of the event centre</param>
        /// <returns>The event centre details</returns>
        /// <response code="200">Event centre retrieved successfully</response>
        /// <response code="404">Event centre not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("get-event-center")]
        [ProducesResponseType(typeof(APIResponse<EventCentreDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid eventCenterId)
        {
            var result = await _eventCentreService.GetEventCentreById(eventCenterId);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Create a new event centre
        /// </summary>
        /// <param name="addEventCentreDto">The event centre details including name, location, and capacity</param>
        /// <returns>The newly created event centre</returns>
        /// <response code="201">Event centre created successfully</response>
        /// <response code="400">Invalid event centre data or duplicate name</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("create-event-center")]
        [ProducesResponseType(typeof(APIResponse<EventCentreDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AddEventCentreDto addEventCentreDto)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.AddEventCentre(userId, addEventCentreDto);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Update an existing event centre's details
        /// </summary>
        /// <param name="eventCenterId">The unique identifier of the event centre to update</param>
        /// <param name="addEventCentreDto">The updated event centre details</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Event centre updated successfully</response>
        /// <response code="400">Invalid update data</response>
        /// <response code="404">Event centre not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPut("update-event-center")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid eventCenterId, [FromBody] AddEventCentreDto addEventCentreDto)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.UpdateEventCentreAsync(userId, eventCenterId, addEventCentreDto);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Soft-delete an event centre by deactivating it
        /// </summary>
        /// <param name="eventCenterId">The unique identifier of the event centre to delete</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Event centre deleted successfully</response>
        /// <response code="404">Event centre not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpDelete("delete-event-center")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid eventCenterId)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.DeleteEventCentre(userId, eventCenterId);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Reactivate a previously deactivated event centre
        /// </summary>
        /// <param name="eventCenterId">The unique identifier of the event centre to reactivate</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Event centre reactivated successfully</response>
        /// <response code="400">Event centre is already active</response>
        /// <response code="404">Event centre not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPut("reactivate-event-center")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReactivateEventCenter(Guid eventCenterId)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.ReactivateEventCentre(userId, eventCenterId);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Add availability schedules to an event centre (e.g., open hours for specific days)
        /// </summary>
        /// <param name="request">List of availability details including day, open time, and close time</param>
        /// <returns>The newly created availability records</returns>
        /// <response code="201">Availabilities added successfully</response>
        /// <response code="400">Invalid availability data or conflicting schedule</response>
        /// <response code="404">Event centre not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("add-event-center-availability")]
        [ProducesResponseType(typeof(APIResponse<List<EventCentreAvailabilityDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddEventCenterAvailabilityDetail([FromBody] List<AddEventCentreAvailabilityDto> request)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.AddEventCentreAvailability(userId, request);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Update an existing availability schedule for an event centre
        /// </summary>
        /// <param name="availabilityId">The unique identifier of the availability record to update</param>
        /// <param name="request">The updated availability details</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Availability updated successfully</response>
        /// <response code="400">Invalid availability data</response>
        /// <response code="404">Availability record not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPut("update-event-center-availability")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEventCenterAvailability(Guid availabilityId, UpdateEventCentreAvailabilityDto request)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.UpdateEventCentreAvailability(userId, availabilityId, request);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Delete an availability schedule from an event centre
        /// </summary>
        /// <param name="availabilityId">The unique identifier of the availability record to delete</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Availability deleted successfully</response>
        /// <response code="404">Availability record not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpDelete("delete-event-center-availability")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEventCenterAvailability(Guid availabilityId)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.DeleteEventCentreAvailability(userId, availabilityId);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
