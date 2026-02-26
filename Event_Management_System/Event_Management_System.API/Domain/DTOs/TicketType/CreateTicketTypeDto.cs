namespace Event_Management_System.API.Domain.DTOs.TicketType
{
    public class CreateTicketTypeDto
    {
        public Guid EventId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Capacity { get; set; } 
        public decimal Price { get; set; } 
        public bool IsActive { get; set; }
    }
}
