namespace Event_Management_System.API.Domain.DTOs.Analytics
{
    public class EventCentreUtilizationDto
    {
        public Guid EventCentreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public double UtilizationRate { get; set; }
    }

    public class EventCentreRevenueDto
    {
        public Guid EventCentreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalConfirmedBookings { get; set; }
        public decimal PricePerDay { get; set; }
    }

    public class EventCentreAvailabilityTrendDto
    {
        public Guid EventCentreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<PeakPeriodDto> PeakPeriods { get; set; } = new();
    }

    public class PeakPeriodDto
    {
        public string Period { get; set; } = string.Empty;
        public int BookingCount { get; set; }
    }
}
