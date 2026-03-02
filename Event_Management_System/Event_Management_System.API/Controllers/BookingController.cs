using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.Booking;
using Event_Management_System.API.Domain.DTOs.EventCenter;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Event_Management_System.API.Controllers
{
    /// <summary>
    /// Manages event centre bookings including creation, retrieval, status updates, and availability checks
    /// </summary>
    [Authorize(Roles = "Organizer, Administrator")]
    [Authorize]
    [Route("bookings")]
    [ApiController]
    public class BookingController : BaseController
    {
        private readonly IBookingService _bookingService;
        public BookingController(IHttpContextAccessor contextAccessor, IConfiguration configuration, IBookingService bookingService) : base(contextAccessor, configuration)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Retrieve all bookings with pagination. Admins see all bookings; organizers see only their own.
        /// </summary>
        /// <param name="requestParameters">Pagination parameters including page number and page size</param>
        /// <param name="bookingFilter">Optional filters for booking status, event centre, and date range</param>
        /// <returns>A paginated list of bookings</returns>
        /// <response code="200">Bookings retrieved successfully</response>
        /// <response code="400">Invalid request parameters</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("get-all")]
        [ProducesResponseType(typeof(APIResponse<PagedList<BookingDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBookings([FromQuery] RequestParameters requestParameters, [FromQuery] BookingFilter bookingFilter)
        {
            var userId = GetUserId();
            var result = await _bookingService.GetAllBookings(userId, requestParameters, bookingFilter);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Retrieve a specific booking by its unique identifier
        /// </summary>
        /// <param name="bookingId">The unique identifier of the booking</param>
        /// <returns>The booking details</returns>
        /// <response code="200">Booking retrieved successfully</response>
        /// <response code="404">Booking not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("get-by-id")]
        [ProducesResponseType(typeof(APIResponse<BookingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBookingById(Guid bookingId)
        {
            var result = await _bookingService.GetBookingById(bookingId);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Create a new booking for an event centre. For paid event centres, a Paystack payment will be initiated
        /// and the response will include a payment URL for the organizer to complete payment.
        /// </summary>
        /// <param name="createBookingDto">The booking details including event centre ID and date range</param>
        /// <returns>
        /// For free event centres: booking confirmation details.
        /// For paid event centres: booking details with Paystack payment URL.
        /// </returns>
        /// <response code="201">Booking created (free) or created with payment URL (paid)</response>
        /// <response code="400">Invalid booking data, event centre unavailable, or scheduling conflict</response>
        /// <response code="404">Event centre not found</response>
        /// <response code="500">An internal server error occurred or payment initialization failed</response>
        [HttpPost("create-booking")]
        [ProducesResponseType(typeof(APIResponse<CreateBookingResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto createBookingDto)
        {
            var userId = GetUserId();
            var result = await _bookingService.CreateBooking(userId, createBookingDto);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Update the status of an existing booking (e.g., Confirmed, Cancelled, Rejected)
        /// </summary>
        /// <param name="bookingStatus">The new booking status</param>
        /// <param name="bookingId">The unique identifier of the booking to update</param>
        /// <returns>No content on success</returns>
        /// <response code="200">Booking status updated successfully</response>
        /// <response code="400">Invalid status transition or booking already in target status</response>
        /// <response code="403">User not authorized to update this booking</response>
        /// <response code="404">Booking not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPut("update-booking-status")]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateBookingStatus([FromQuery] BookingStatus bookingStatus, [FromQuery] Guid bookingId)
        {
            var userId = GetUserId();
            var result = await _bookingService.UpdateBookingStatusByAdmin(userId, bookingStatus, bookingId);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Retrieve available event centres for a specified date and time range
        /// </summary>
        /// <param name="bookedFrom">The desired booking start date and time</param>
        /// <param name="bookedTo">The desired booking end date and time</param>
        /// <returns>A list of available event centres</returns>
        /// <response code="200">Available event centres retrieved successfully</response>
        /// <response code="400">Invalid date range (e.g., start date in the past or end before start)</response>
        /// <response code="404">No available event centres found for the specified date range</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("get-available-centers")]
        [ProducesResponseType(typeof(APIResponse<List<EventCentreDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAvailableEventCentres([FromQuery] DateTime bookedFrom, [FromQuery] DateTime bookedTo)
        {
            var result = await _bookingService.GetAvailableEventCentres(bookedFrom, bookedTo);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Permanently delete a booking record
        /// </summary>
        /// <param name="bookingId">The unique identifier of the booking to delete</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Booking deleted successfully</response>
        /// <response code="404">Booking or user not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpDelete("delete-booking")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBooking(Guid bookingId)
        {
            var userId = GetUserId();
            var result = await _bookingService.DeleteBookingAsync(userId, bookingId);
            return StatusCode((int)result.StatusCode, result);  
        }
    }
}
