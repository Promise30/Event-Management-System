namespace Event_Management_System.API.Domain.DTOs.Analytics
{
    public class TicketsSoldDto
    {
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public Guid? EventId { get; set; }
    }

    public class TicketsByTypeDto
    {
        public Guid TicketTypeId { get; set; }
        public string TicketTypeName { get; set; } = string.Empty;
        public Guid EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Sold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TicketAvailabilityDto
    {
        public Guid EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public int TotalCapacity { get; set; }
        public int TotalSold { get; set; }
        public int Remaining { get; set; }
        public double SoldPercentage { get; set; }
        public List<TicketTypeAvailabilityDto> TicketTypes { get; set; } = new();
    }

    public class TicketTypeAvailabilityDto
    {
        public Guid TicketTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int Sold { get; set; }
        public int Remaining { get; set; }
        public decimal Price { get; set; }
    }
}
