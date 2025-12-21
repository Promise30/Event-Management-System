namespace Event_Management_System.API.Domain.DTOs.Payment
{
    public class InitiatePaymentDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "NGN";
        public string Description { get; set; }
        public string RedirectUrl { get; set; }
        public PaymentType PaymentType { get; set; }
        public Guid ReferenceId { get; set; } // BookingId or TicketId
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class PaymentInitializationDto
    {
        public string TransactionReference { get; set; }
        public string PaymentUrl { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Provider { get; set; }
    }

    public class PaymentVerificationDto
    {
        public string TransactionReference { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string CustomerEmail { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class RefundDto
    {
        public string RefundReference { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public enum PaymentType
    {
        Booking = 1,
        Ticket = 2
    }
}
