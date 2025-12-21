namespace Event_Management_System.API.Domain.DTOs
{
    public class CreateTicketDto
    {
        public Guid TicketTypeId { get; set; }
        public Guid EventId { get; set; }

    }
}
