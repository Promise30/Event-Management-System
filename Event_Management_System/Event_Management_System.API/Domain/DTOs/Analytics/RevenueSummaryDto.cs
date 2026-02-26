namespace Event_Management_System.API.Domain.DTOs.Analytics
{
    public class RevenueSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalPayments { get; set; }
        public decimal BookingRevenue { get; set; }
        public decimal TicketRevenue { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class RevenueByEventDto
    {
        public Guid EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TicketsSold { get; set; }
    }

    public class RevenueByPeriodDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TransactionCount { get; set; }
    }

    public class BookingSummaryDto
    {
        public int TotalBookings { get; set; }
        public int Confirmed { get; set; }
        public int PendingPayment { get; set; }
        public int PendingApproval { get; set; }
        public int Cancelled { get; set; }
        public int Rejected { get; set; }
        public int Expired { get; set; }
        public int Submitted { get; set; }
    }

    public class BookingsByEventDto
    {
        public Guid EventCentreId { get; set; }
        public string EventCentreName { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int Confirmed { get; set; }
        public int Cancelled { get; set; }
    }

    public class BookingConversionRateDto
    {
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public double ConversionRate { get; set; }
        public int TotalTicketReservations { get; set; }
        public int ConfirmedTickets { get; set; }
        public double TicketConversionRate { get; set; }
    }
}
