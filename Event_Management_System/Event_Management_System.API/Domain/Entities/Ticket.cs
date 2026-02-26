using Event_Management_System.API.Domain.Enums;

namespace Event_Management_System.API.Domain.Entities
{
    public class Ticket : BaseEntity<Guid>
    {
        public Guid TicketTypeId { get; set; }
        public TicketType TicketType { get; set; }
        //public Guid EventId { get; set; }
        //public Event Event { get; set; }

        public Guid AttendeeId { get; set; }
        public ApplicationUser Attendee { get; set; }

        public string TicketNumber { get; set; } = string.Empty; // unique ticket number e.g "Event-No-Count"
        public TicketStatus Status { get; set; } 
        public string? PaymentReference { get; set; } // for paid tickets
        public DateTimeOffset? ReservationExpiresAt { get; set; }
        public DateTimeOffset? PaymentCompletedAt { get; set; }
    }
}

// free, paid event
// after creating event, provide ticket types

// Ticket number strategy: EVT-{EventId}-{yyyyMMdd}-{RandomString}
// RandomString: 4-6 alphanumeric characters