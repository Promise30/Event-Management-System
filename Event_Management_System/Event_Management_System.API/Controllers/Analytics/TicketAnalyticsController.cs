using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.Analytics;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management_System.API.Controllers.Analytics
{
    /// <summary>
    /// Provides ticket analytics endpoints for administrators
    /// </summary>
    [Authorize(Roles = "Administrator")]
    [Route("api/analytics/tickets")]
    [ApiController]
    public class TicketAnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public TicketAnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Get total tickets sold, optionally filtered by event or date range
        /// </summary>
        /// <param name="eventId">Optional event ID filter</param>
        /// <param name="from">Start date filter (inclusive)</param>
        /// <param name="to">End date filter (inclusive)</param>
        /// <returns>Ticket sales summary</returns>
        /// <response code="200">Tickets sold data retrieved successfully</response>
        [HttpGet("sold")]
        [ProducesResponseType(typeof(APIResponse<TicketsSoldDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTicketsSold([FromQuery] Guid? eventId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var result = await _analyticsService.GetTicketsSoldAsync(eventId, from, to);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Get sales breakdown per ticket type, optionally filtered by event
        /// </summary>
        /// <param name="eventId">Optional event ID filter</param>
        /// <returns>Ticket sales per type</returns>
        /// <response code="200">Tickets by type retrieved successfully</response>
        [HttpGet("by-type")]
        [ProducesResponseType(typeof(APIResponse<List<TicketsByTypeDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTicketsByType([FromQuery] Guid? eventId)
        {
            var result = await _analyticsService.GetTicketsByTypeAsync(eventId);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Get remaining vs sold tickets per event, optionally filtered by event
        /// </summary>
        /// <param name="eventId">Optional event ID filter</param>
        /// <returns>Ticket availability per event with breakdown by ticket type</returns>
        /// <response code="200">Ticket availability retrieved successfully</response>
        [HttpGet("availability")]
        [ProducesResponseType(typeof(APIResponse<List<TicketAvailabilityDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTicketAvailability([FromQuery] Guid? eventId)
        {
            var result = await _analyticsService.GetTicketAvailabilityAsync(eventId);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
