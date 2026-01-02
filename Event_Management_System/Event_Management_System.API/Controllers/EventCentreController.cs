using Event_Management_System.API.Application;
using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Event_Management_System.API.Controllers
{
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

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] RequestParameters requestParameters)
        {
            var result = await _eventCentreService.GetAllEventCentresAsync(requestParameters);
            return result.StatusCode == HttpStatusCode.OK ? Ok(result) : StatusCode((int)result.StatusCode, result);
        }
        [HttpGet("get-event-center")]
        public async Task<IActionResult> GetById(Guid eventCenterId)
        {
            var result = await _eventCentreService.GetEventCentreById(eventCenterId);
            return result.StatusCode == HttpStatusCode.OK ? Ok(result) : StatusCode((int)result.StatusCode, result);
        }
        [HttpPost("create-event-center")]
        public async Task<IActionResult> Create([FromBody] AddEventCentreDto addEventCentreDto)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.AddEventCentre(userId, addEventCentreDto);
            return result.StatusCode == HttpStatusCode.Created ? CreatedAtAction(nameof(GetById), result.Data) : StatusCode((int)result.StatusCode, result);
        }
        [HttpPut("update-event-center")]
        public async Task<IActionResult> Update(Guid eventCenterId, [FromBody] AddEventCentreDto addEventCentreDto)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.UpdateEventCentreAsync(userId, eventCenterId, addEventCentreDto);
            return result.StatusCode == HttpStatusCode.NoContent ? NoContent() : StatusCode((int)result.StatusCode, result);
        }
        [HttpDelete("delete-event-center")]
        public async Task<IActionResult> Delete(Guid eventCenterId)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.DeleteEventCentre(userId, eventCenterId);
            return result.StatusCode == HttpStatusCode.NoContent ? NoContent() : StatusCode((int)result.StatusCode, result);

        }
        [HttpPut("reactivate-event-center")]
        public async Task<IActionResult> ReactivateEventCenter(Guid eventCenterId)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.ReactivateEventCentre(userId, eventCenterId);
            return result.StatusCode == HttpStatusCode.NoContent ? NoContent() : StatusCode((int)result.StatusCode, result);

        }
        [HttpPost("add-event-center-availability")]
        public async Task<IActionResult> AddEventCenterAvailabilityDetail(AddEventCentreAvailabilityDto request)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.AddEventCentreAvailability(userId, request);
            return result.StatusCode == HttpStatusCode.Created ? CreatedAtAction(nameof(GetById), result.Data) : StatusCode((int)result.StatusCode, result);

        }
        [HttpPut("update-event-center-availability")]
        public async Task<IActionResult> UpdateEventCenterAvailability(Guid availabilityId, UpdateEventCentreAvailabilityDto request)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.UpdateEventCentreAvailability(userId, availabilityId, request);
            return result.StatusCode == HttpStatusCode.NoContent ? NoContent() : StatusCode((int)result.StatusCode, result);
        }
        [HttpDelete("delete-event-center-availability")]
        public async Task<IActionResult> DeleteEventCenterAvailability(Guid availabilityId)
        {
            var userId = GetUserId();
            var result = await _eventCentreService.DeleteEventCentreAvailability(userId, availabilityId);
            return result.StatusCode == HttpStatusCode.NoContent ? NoContent() : StatusCode((int)result.StatusCode, result);
        }



    }
}
