using Event_Management_System.API.Application;
using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Event_Management_System.API.Controllers
{
    //[Authorize(Roles = "Organizer, Administrator")]
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
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllBookings([FromQuery] RequestParameters requestParameters, [FromQuery] BookingFilter bookingFilter)
        {
            var userId = GetUserId();
            var result = await _bookingService.GetAllBookings(userId, requestParameters, bookingFilter);
            return result.StatusCode == HttpStatusCode.OK ? Ok(result) : StatusCode((int)result.StatusCode, result);
        }
        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetBookingById(Guid bookingId)
        {
            var result = await _bookingService.GetBookingById(bookingId);
            return result.StatusCode == HttpStatusCode.OK ? Ok(result) : StatusCode((int)result.StatusCode, result);
        }
        [HttpPost("create-booking")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto createBookingDto)
        {
            var userId = GetUserId();
            var result = await _bookingService.CreateBooking(userId, createBookingDto);
            return result.StatusCode == HttpStatusCode.Created ? CreatedAtAction(nameof(GetBookingById), result) : StatusCode((int)result.StatusCode, result);
        }
        [HttpPut("update-booking-status")]
        public async Task<IActionResult> UpdateBookingStatus([FromQuery] BookingStatus bookingStatus, [FromQuery] Guid bookingId)
        {
            var userId = GetUserId();
            var result = await _bookingService.UpdateBookingStatus(userId, bookingStatus, bookingId);
            return result.StatusCode == HttpStatusCode.NoContent ? NoContent() : StatusCode((int)result.StatusCode, result);
        }
        [HttpGet("get-available-centers")]
        public async Task<IActionResult> GetAvailableEventCentres([FromQuery] DateTime bookedFrom, [FromQuery] DateTime bookedTo)
        {
            var result = await _bookingService.GetAvailableEventCentres(bookedFrom, bookedTo);
            return result.StatusCode == HttpStatusCode.OK ? Ok(result) : StatusCode((int)result.StatusCode, result);
        }
        [HttpDelete("delete-booking")]
        public async Task<IActionResult> DeleteBooking(Guid bookingId)
        {
            var userId = GetUserId();
            var result = await _bookingService.DeleteBookingAsync(userId, bookingId);
            return result.StatusCode == HttpStatusCode.NoContent ? NoContent() : StatusCode((int)result.StatusCode, result);
        }
    }
}
