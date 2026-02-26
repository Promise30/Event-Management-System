using Event_Management_System.API.Domain.Entities;

namespace Event_Management_System.API.Domain.DTOs.TicketType
{
    public class TicketTypeDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }

        public string Name { get; set; }
        public string? Description { get; set; }
        public int Capacity { get; set; } 
        public decimal Price { get; set; } 
        public bool IsActive { get; set; }
        public DateTimeOffset DateCreated { get; set; }

    }
}
