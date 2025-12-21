namespace Event_Management_System.API.Domain.Entities
{
    public class Event : BaseEntity<Guid>
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? EventFlyer { get; set; }
        public int NumberOfAttendees { get; set; }
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; }
        //public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
    }
}
