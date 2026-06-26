using MyFreelance.Application.DTOs.Notifications;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Application.Interfaces;

public interface INotificationService
{
    Task SendAsync(string? userId, NotificationEventType eventType, NotificationChannel channel, string title, string message, CancellationToken cancellationToken = default);
    Task SendEventNotificationAsync(string userId, NotificationEventType eventType, Dictionary<string, string>? placeholders = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
}
