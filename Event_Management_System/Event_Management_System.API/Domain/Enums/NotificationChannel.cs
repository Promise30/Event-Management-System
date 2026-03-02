using System.ComponentModel;

namespace Event_Management_System.API.Domain.Enums
{
    public enum NotificationChannel
    {
        [Description("Email")]
        Email = 1,
        [Description("SMS")]
        SMS = 2,
    }
}
