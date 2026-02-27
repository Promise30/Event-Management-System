using Event_Management_System.API.Domain.Enums;

namespace Event_Management_System.API.Domain.DTOs
{
    public class NotificationRequest
    {
        public string RecipientEmail { get; set; }
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; } // for SMS
        public NotificationType Type { get; set; }
        public NotificationChannel[] Channels { get; set; } // caller decides
        public Dictionary<string, string> Data { get; set; } = new();
    }
}
