namespace Event_Management_System.API.Domain.DTOs.Analytics
{
    public class PopularEventDto
    {
        public Guid EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class UpcomingLowAvailabilityEventDto
    {
        public Guid EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int TotalCapacity { get; set; }
        public int TotalSold { get; set; }
        public int Remaining { get; set; }
        public double SoldPercentage { get; set; }
    }

    public class EventPerformanceDto
    {
        public Guid EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TotalCapacity { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public double SoldPercentage { get; set; }
    }
}
