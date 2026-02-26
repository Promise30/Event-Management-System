namespace Event_Management_System.API.Domain.DTOs.Payment
{
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
}
