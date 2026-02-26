using Event_Management_System.API.Domain.Enums;

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

}
