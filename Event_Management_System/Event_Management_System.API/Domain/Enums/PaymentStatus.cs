using System.ComponentModel;

namespace Event_Management_System.API.Domain.Enums
{
    public enum PaymentStatus
    {
        [Description("Pending")]
        Pending = 0,
        
        [Description("Processing")]
        Processing = 1,
        
        [Description("Successful")]
        Successful = 2,
        
        [Description("Failed")]
        Failed = 3,
        
        [Description("Cancelled")]
        Cancelled = 4,
        
        [Description("Refunded")]
        Refunded = 5,
        
        [Description("PartiallyRefunded")]
        PartiallyRefunded = 6
    }
}
