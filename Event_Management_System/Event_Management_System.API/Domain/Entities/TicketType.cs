using Event_Management_System.API.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Management_System.API.Domain.Entities
{
    public class TicketType : BaseEntity<Guid>
    {
        public Guid EventId { get; set; }
        public Event Event { get; set; }

        public string Name { get; set; }
        public string? Description { get; set; }
        public int Capacity { get; set; } // total tickets available for this type
        public int SoldCount { get; set; }
        public decimal Price { get; set; } // 0 represents free tickets
        public bool IsActive { get; set; }

        //[NotMapped] - not ideal as it loads all the tickets into memory and does the count. Can cause performance issue
        //public int AvailableTickets => Capacity - Tickets.Count(t => t.Status != TicketStatus.Cancelled);
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        // Sale Period Constraint
        public DateTime? SaleStartDate { get; set; }  // When tickets go on sale
        public DateTime? SaleEndDate { get; set; }
    }
}
