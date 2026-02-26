using System.ComponentModel;

namespace Event_Management_System.API.Domain.Enums
{
    public enum OrganizerRequestStatus
    {
        [Description("Pending")]
        Pending = 0,

        [Description("Approved")]
        Approved = 1,

        [Description("Rejected")]
        Rejected = 2
    }
}
