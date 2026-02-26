namespace Event_Management_System.API.Domain.DTOs.Payment
{
    public class RefundDto
    {
        public string RefundReference { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
