using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.Analytics;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Event_Management_System.API.Controllers.Analytics
{
    /// <summary>
    /// Provides revenue and booking analytics endpoints for administrators
    /// </summary>
    [Authorize(Roles = "Administrator")]
    [Route("api/analytics")]
    [ApiController]
    public class RevenueAnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public RevenueAnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Get total revenue summary over an optional date range
        /// </summary>
        /// <param name="from">Start date filter (inclusive)</param>
        /// <param name="to">End date filter (inclusive)</param>
        /// <returns>Revenue summary including total, booking, and ticket revenue</returns>
        /// <response code="200">Revenue summary retrieved successfully</response>
        [HttpGet("revenue/summary")]
        [ProducesResponseType(typeof(APIResponse<RevenueSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRevenueSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var result = await _analyticsService.GetRevenueSummaryAsync(from, to);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Get revenue broken down per event
        /// </summary>
        /// <returns>List of revenue data per event</returns>
        /// <response code="200">Revenue by event retrieved successfully</response>
        [HttpGet("revenue/by-event")]
        [ProducesResponseType(typeof(APIResponse<List<RevenueByEventDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRevenueByEvent()
        {
            var result = await _analyticsService.GetRevenueByEventAsync();
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Get revenue grouped by day, week, or month
        /// </summary>
        /// <param name="groupBy">Grouping period: "day" (default), "week", or "month"</param>
        /// <returns>List of revenue data per period</returns>
        /// <response code="200">Revenue by period retrieved successfully</response>
        [HttpGet("revenue/by-period")]
        [ProducesResponseType(typeof(APIResponse<List<RevenueByPeriodDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRevenueByPeriod([FromQuery] string groupBy = "day")
        {
            var result = await _analyticsService.GetRevenueByPeriodAsync(groupBy);
            return StatusCode((int)result.StatusCode, result);  
        }

        /// <summary>
        /// Get total bookings summary with status breakdown
        /// </summary>
        /// <param name="from">Start date filter (inclusive)</param>
        /// <param name="to">End date filter (inclusive)</param>
        /// <returns>Booking counts by status</returns>
        /// <response code="200">Booking summary retrieved successfully</response>
        [HttpGet("bookings/summary")]
        [ProducesResponseType(typeof(APIResponse<BookingSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookingSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var result = await _analyticsService.GetBookingSummaryAsync(from, to);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Get booking counts per event centre
        /// </summary>
        /// <returns>List of booking counts per event centre</returns>
        /// <response code="200">Bookings by event centre retrieved successfully</response>
        [HttpGet("bookings/by-event")]
        [ProducesResponseType(typeof(APIResponse<List<BookingsByEventDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookingsByEvent()
        {
            var result = await _analyticsService.GetBookingsByEventAsync();
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Get booking and ticket conversion rates (confirmed vs total)
        /// </summary>
        /// <returns>Conversion rate statistics</returns>
        /// <response code="200">Conversion rate retrieved successfully</response>
        [HttpGet("bookings/conversion-rate")]
        [ProducesResponseType(typeof(APIResponse<BookingConversionRateDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookingConversionRate()
        {
            var result = await _analyticsService.GetBookingConversionRateAsync();
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
