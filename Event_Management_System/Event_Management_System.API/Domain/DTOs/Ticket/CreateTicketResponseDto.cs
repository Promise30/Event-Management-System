namespace Event_Management_System.API.Domain.DTOs.Ticket
{
    public class CreateTicketResponseDto
    {
        public Guid TicketId { get; set; }
        public string TicketStatus { get; set; }
        public string TicketNumber { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentUrl { get; set; }
        public string? PaymentReference { get; set; }
        public DateTimeOffset DateCreated { get; set; }

    }
}
