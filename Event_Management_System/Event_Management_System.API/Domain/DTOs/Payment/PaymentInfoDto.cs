namespace Event_Management_System.API.Domain.DTOs.Payment
{
    public class PaymentInfoDto
    {
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;   
        public string PaymentProvider { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset? PaidAt { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
