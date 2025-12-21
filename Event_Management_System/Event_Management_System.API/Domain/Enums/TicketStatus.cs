using System.ComponentModel;

namespace Event_Management_System.API.Domain.Enums
{
    public enum TicketStatus
    {
        [Description("Pending")]
        Pending = 0,
        [Description("Active")]
        Active = 1,
        [Description("Confirmed")]
        Confirmed = 2,
        [Description("Used")]
        Used = 3,
        [Description("Cancelled")]
        Cancelled = 4,
        [Description("Expired")]
        Expired = 5,
        [Description("Reserved")]
        Reserved = 6
    }
}
