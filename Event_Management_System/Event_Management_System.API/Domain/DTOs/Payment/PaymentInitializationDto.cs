using Event_Management_System.API.Domain.Enums;

namespace Event_Management_System.API.Domain.DTOs.Payment
{

    public class PaymentInitializationDto
    {
        public string TransactionReference { get; set; }
        public string PaymentUrl { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Provider { get; set; }
    }

    

    
}
