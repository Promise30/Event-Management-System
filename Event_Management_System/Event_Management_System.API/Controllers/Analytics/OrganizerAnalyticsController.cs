using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.Analytics;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management_System.API.Controllers.Analytics
{
    /// <summary>
    /// Provides organizer analytics endpoints for administrators
    /// </summary>
    [Authorize(Roles = "Administrator")]
    [Route("api/analytics/organizers")]
    [ApiController]
    public class OrganizerAnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public OrganizerAnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Get revenue and booking performance per organizer
        /// </summary>
        /// <param name="organizerId">Optional organizer ID to filter for a specific organizer</param>
        /// <returns>List of organizer performance summaries</returns>
        /// <response code="200">Organizer performance data retrieved successfully</response>
        [HttpGet("performance")]
        [ProducesResponseType(typeof(APIResponse<List<OrganizerPerformanceDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrganizerPerformance([FromQuery] Guid? organizerId)
        {
            var result = await _analyticsService.GetOrganizerPerformanceAsync(organizerId);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Get the number of events hosted per organizer with event details
        /// </summary>
        /// <param name="organizerId">Optional organizer ID to filter for a specific organizer</param>
        /// <returns>List of organizers with their hosted events</returns>
        /// <response code="200">Organizer events hosted retrieved successfully</response>
        [HttpGet("events-hosted")]
        [ProducesResponseType(typeof(APIResponse<List<OrganizerEventsHostedDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrganizerEventsHosted([FromQuery] Guid? organizerId)
        {
            var result = await _analyticsService.GetOrganizerEventsHostedAsync(organizerId);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Get a leaderboard of top-performing organizers ranked by revenue and events hosted
        /// </summary>
        /// <param name="top">Number of top organizers to return (default: 10)</param>
        /// <returns>Ranked list of top organizers</returns>
        /// <response code="200">Top organizers retrieved successfully</response>
        [HttpGet("top")]
        [ProducesResponseType(typeof(APIResponse<List<TopOrganizerDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopOrganizers([FromQuery] int top = 10)
        {
            var result = await _analyticsService.GetTopOrganizersAsync(top);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
