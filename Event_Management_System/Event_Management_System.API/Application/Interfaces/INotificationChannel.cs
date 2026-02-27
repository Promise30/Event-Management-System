using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Domain.Enums;

namespace Event_Management_System.API.Application.Interfaces
{
    /// <summary>
    /// Represents a notification delivery channel (e.g., Email, SMS)
    /// </summary>
    public interface INotificationChannel
    {
        /// <summary>
        /// Gets the type of notification channel
        /// </summary>
        NotificationChannel ChannelType { get; }

        /// <summary>
        /// Sends a notification asynchronously
        /// </summary>
        /// <param name="request">The notification request to send</param>
        Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
    }
}
