namespace Event_Management_System.API.Domain.DTOs.Analytics
{
    public class OrganizerPerformanceDto
    {
        public Guid OrganizerId { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int TotalEventsHosted { get; set; }
    }

    public class OrganizerEventsHostedDto
    {
        public Guid OrganizerId { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public int TotalEventsHosted { get; set; }
        public List<OrganizerEventSummaryDto> Events { get; set; } = new();
    }

    public class OrganizerEventSummaryDto
    {
        public Guid EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
    }

    public class TopOrganizerDto
    {
        public int Rank { get; set; }
        public Guid OrganizerId { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int TotalEventsHosted { get; set; }
    }
}
