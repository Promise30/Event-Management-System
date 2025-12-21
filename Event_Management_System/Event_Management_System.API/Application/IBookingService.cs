using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application
{
    public interface IBookingService
    {
        Task<APIResponse<BookingDto>> CreateBooking(Guid userId, CreateBookingDto createBookingDto);
        Task<APIResponse<object>> DeleteBookingAsync(Guid userId, Guid bookingId);
        Task<APIResponse<PagedList<BookingDto>>> GetAllBookings(Guid userId, RequestParameters requestParameters, BookingFilter bookingFilter);
        Task<APIResponse<object>> GetAvailableEventCentres(DateTime bookedFrom, DateTime bookedTo);
        Task<APIResponse<object>> GetBookingById(Guid bookingId);
        Task<APIResponse<object>> GetBookingsByUser(Guid userId);
        Task<APIResponse<object>> UpdateBookingStatus(Guid userId, BookingStatus bookingStatus, Guid bookingId);
    }
}
