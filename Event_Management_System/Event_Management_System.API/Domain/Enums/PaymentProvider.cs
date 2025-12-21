using System.ComponentModel;

namespace Event_Management_System.API.Domain.Enums
{
    public enum PaymentProvider
    {
        [Description("PayStack")]
        PayStack = 1,
        [Description("Flutterwave")]
        Flutterwave = 2,
        [Description("Interswitch")]
        Interswitch = 3
    }
}
