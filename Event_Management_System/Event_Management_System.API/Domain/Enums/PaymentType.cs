using System.ComponentModel;

namespace Event_Management_System.API.Domain.Enums
{
    public enum PaymentType
    {
        [Description("Booking")]
        Booking = 1,
        [Description("Ticket")]
        Ticket = 2
    }
}
