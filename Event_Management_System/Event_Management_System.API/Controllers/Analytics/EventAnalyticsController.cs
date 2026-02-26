using Azure;
using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.Analytics;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Event_Management_System.API.Controllers.Analytics
{
    /// <summary>
    /// Provides event analytics endpoints for administrators
    /// </summary>
    [Authorize(Roles = "Administrator")]
    [Route("api/analytics/events")]
    [ApiController]
    public class EventAnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public EventAnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Get top events ranked by tickets sold or revenue
        /// </summary>
        /// <param name="top">Number of top events to return (default: 10)</param>
        /// <param name="sortBy">Sort criteria: "tickets" (default) or "revenue"</param>
        /// <returns>List of popular events</returns>
        /// <response code="200">Popular events retrieved successfully</response>
        [HttpGet("popular")]
        [ProducesResponseType(typeof(APIResponse<List<PopularEventDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPopularEvents([FromQuery]PopularEventsRequestParameter requestParameter)
        {
            var result = await _analyticsService.GetPopularEventsAsync(requestParameter);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Get upcoming events with low remaining ticket availability
        /// </summary>
        /// <param name="threshold">Minimum sold percentage to flag (default: 80). Events with sold percentage at or above this threshold are returned.</param>
        /// <returns>List of upcoming events with low availability</returns>
        /// <response code="200">Upcoming low-availability events retrieved successfully</response>
        [HttpGet("upcoming")]
        [ProducesResponseType(typeof(APIResponse<List<UpcomingLowAvailabilityEventDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUpcomingLowAvailabilityEvents([FromQuery] double threshold = 80, [FromQuery]DateTime? startDate = null, [FromQuery]DateTime? endDate = null)
        {
            var response = await _analyticsService.GetUpcomingLowAvailabilityEventsAsync(threshold, startDate, endDate);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Get per-event performance summary including capacity, sold tickets, and revenue
        /// </summary>
        /// <param name="eventId">Optional event ID to get performance for a specific event</param>
        /// <returns>List of event performance summaries</returns>
        /// <response code="200">Event performance data retrieved successfully</response>
        [HttpGet("performance")]
        [ProducesResponseType(typeof(APIResponse<List<EventPerformanceDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventPerformance([FromQuery] Guid? eventId)
        {
            var response = await _analyticsService.GetEventPerformanceAsync(eventId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
