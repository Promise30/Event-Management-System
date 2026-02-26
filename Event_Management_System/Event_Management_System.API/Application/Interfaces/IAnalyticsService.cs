using Event_Management_System.API.Domain.DTOs.Analytics;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application.Interfaces
{
    public interface IAnalyticsService
    {
        // Revenue & Bookings Analytics
        Task<APIResponse<RevenueSummaryDto>> GetRevenueSummaryAsync(DateTime? from, DateTime? to);
        Task<APIResponse<List<RevenueByEventDto>>> GetRevenueByEventAsync();
        Task<APIResponse<List<RevenueByPeriodDto>>> GetRevenueByPeriodAsync(string groupBy);
        Task<APIResponse<BookingSummaryDto>> GetBookingSummaryAsync(DateTime? from, DateTime? to);
        Task<APIResponse<List<BookingsByEventDto>>> GetBookingsByEventAsync();
        Task<APIResponse<BookingConversionRateDto>> GetBookingConversionRateAsync();

        // Ticket Analytics
        Task<APIResponse<TicketsSoldDto>> GetTicketsSoldAsync(Guid? eventId, DateTime? from, DateTime? to);
        Task<APIResponse<List<TicketsByTypeDto>>> GetTicketsByTypeAsync(Guid? eventId);
        Task<APIResponse<List<TicketAvailabilityDto>>> GetTicketAvailabilityAsync(Guid? eventId);

        // Event Analytics
        Task<APIResponse<List<PopularEventDto>>> GetPopularEventsAsync(PopularEventsRequestParameter requestParameter);
        Task<APIResponse<List<UpcomingLowAvailabilityEventDto>>> GetUpcomingLowAvailabilityEventsAsync(double threshold, DateTime? startDate, DateTime? endDate);
        Task<APIResponse<List<EventPerformanceDto>>> GetEventPerformanceAsync(Guid? eventId);

        // Event Centre Analytics
        Task<APIResponse<List<EventCentreUtilizationDto>>> GetEventCentreUtilizationAsync();
        Task<APIResponse<List<EventCentreRevenueDto>>> GetEventCentreRevenueAsync();
        Task<APIResponse<List<EventCentreAvailabilityTrendDto>>> GetEventCentreAvailabilityTrendsAsync(string groupBy);

        // Organizer Analytics
        Task<APIResponse<List<OrganizerPerformanceDto>>> GetOrganizerPerformanceAsync(Guid? organizerId);
        Task<APIResponse<List<OrganizerEventsHostedDto>>> GetOrganizerEventsHostedAsync(Guid? organizerId);
        Task<APIResponse<List<TopOrganizerDto>>> GetTopOrganizersAsync(int top);
    }
}
