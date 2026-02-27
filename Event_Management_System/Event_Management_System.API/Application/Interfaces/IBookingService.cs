using Event_Management_System.API.Domain.DTOs.Booking;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application.Interfaces
{
    public interface IBookingService
    {
        Task<APIResponse<CreateBookingResponseDto>> CreateBooking(Guid userId, CreateBookingDto createBookingDto);
        Task<APIResponse<object>> DeleteBookingAsync(Guid userId, Guid bookingId);
        Task<APIResponse<PagedList<BookingDto>>> GetAllBookings(Guid userId, RequestParameters requestParameters, BookingFilter bookingFilter);
        Task<APIResponse<object>> GetAvailableEventCentres(DateTime bookedFrom, DateTime bookedTo);
        Task<APIResponse<object>> GetBookingById(Guid bookingId);
        Task<APIResponse<object>> GetBookingsByUser(Guid userId);
        Task<APIResponse<object>> UpdateBookingStatusByAdmin(Guid userId, BookingStatus bookingStatus, Guid bookingId);
        Task<APIResponse<object>> CancelBookingAsync(Guid userId, Guid bookingId);
    }
}
