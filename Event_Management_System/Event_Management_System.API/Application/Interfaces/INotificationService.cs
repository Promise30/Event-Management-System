using Event_Management_System.API.Domain.DTOs;

namespace Event_Management_System.API.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendAsync(NotificationRequest request);
    }
}
