using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs;

namespace Event_Management_System.API.Application.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IEnumerable<INotificationChannel> _channels;

        public NotificationService(ILogger<NotificationService> logger, IEnumerable<INotificationChannel> channels)
        {
            _logger = logger;
            _channels = channels;
        }

        public async Task SendAsync(NotificationRequest request)
        {
            var targetedChannels = request.Channels?.Length > 0
                ? _channels.Where(c => request.Channels.Contains(c.ChannelType))
                : _channels;

            foreach (var channel in targetedChannels)
            {
                try
                {
                    await channel.SendAsync(request);
                }
                catch (Exception ex)
                {
                    // Log but don't let a failed notification break the main flow
                    _logger.LogError(ex,
                        "Notification channel {Channel} failed for {Type} to {Email}",
                        channel.GetType().Name,
                        request.Type,
                        request.RecipientEmail);
                }
            }
        }
    }
}
