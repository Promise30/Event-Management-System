namespace Event_Management_System.API.Domain.DTOs.Ticket
{
    public class TicketDto
    {
        public Guid Id { get; set; }
        public string TicketType { get; set; }
        public string TicketNumber { get; set; }
        public Guid AttendeeId { get; set; }
        public string TicketStatus { get; set; }
        public DateTimeOffset DateCreated { get; set; }
    }
}
