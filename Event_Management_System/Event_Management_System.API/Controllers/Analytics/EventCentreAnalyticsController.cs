using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.Analytics;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management_System.API.Controllers.Analytics
{
    /// <summary>
    /// Provides event centre analytics endpoints for administrators
    /// </summary>
    [Authorize(Roles = "Administrator")]
    [Route("api/analytics/event-centres")]
    [ApiController]
    public class EventCentreAnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public EventCentreAnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Get utilization rates for each event centre based on booking history
        /// </summary>
        /// <returns>List of event centres with utilization metrics</returns>
        /// <response code="200">Event centre utilization retrieved successfully</response>
        [HttpGet("utilization")]
        [ProducesResponseType(typeof(APIResponse<List<EventCentreUtilizationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventCentreUtilization()
        {
            var result = await _analyticsService.GetEventCentreUtilizationAsync();
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Get revenue generated per event centre from confirmed bookings
        /// </summary>
        /// <returns>List of event centres with revenue data</returns>
        /// <response code="200">Event centre revenue retrieved successfully</response>
        [HttpGet("revenue")]
        [ProducesResponseType(typeof(APIResponse<List<EventCentreRevenueDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventCentreRevenue()
        {
            var result = await _analyticsService.GetEventCentreRevenueAsync();
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Get peak booking periods per event centre
        /// </summary>
        /// <param name="groupBy">Grouping period: "month" (default) or "dayofweek"</param>
        /// <returns>List of event centres with peak booking periods</returns>
        /// <response code="200">Availability trends retrieved successfully</response>
        [HttpGet("availability-trends")]
        [ProducesResponseType(typeof(APIResponse<List<EventCentreAvailabilityTrendDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventCentreAvailabilityTrends([FromQuery] string groupBy = "month")
        {
            var result = await _analyticsService.GetEventCentreAvailabilityTrendsAsync(groupBy);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
