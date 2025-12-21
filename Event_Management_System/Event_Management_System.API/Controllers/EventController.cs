using Event_Management_System.API.Application;
using Event_Management_System.API.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management_System.API.Controllers
{
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
        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetEventById(Guid eventId)
        {
            var response = await _eventService.GetEventById(eventId);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllEvents([FromQuery] RequestParameters requestParameters)
        {
            var response = await _eventService.GetAllEvents(requestParameters);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPost("create-event")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto createEventDto)
        {
            var userId = GetUserId();
            var response = await _eventService.CreateEventAsync(userId, createEventDto);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPut("update-event")]
        public async Task<IActionResult> UpdateEvent([FromQuery] Guid eventId, [FromBody] UpdateEventDto updateEventDto)
        {
            var userId = GetUserId();
            var response = await _eventService.UpdateEventAsync(userId, eventId, updateEventDto);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpDelete("delete-event")]
        public async Task<IActionResult> DeleteEvent([FromQuery] Guid eventId)
        {
            var userId = GetUserId();
            var response = await _eventService.DeleteEventAsync(userId, eventId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
