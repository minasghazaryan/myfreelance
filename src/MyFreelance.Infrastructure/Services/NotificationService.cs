using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Notifications;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Domain.Interfaces;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class NotificationService(ApplicationDbContext db, IUnitOfWork unitOfWork) : INotificationService
{
    public async Task SendAsync(string? userId, NotificationEventType eventType, NotificationChannel channel, string title, string message, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            EventType = eventType,
            Channel = channel,
            Title = title,
            Message = message,
            IsSent = channel != NotificationChannel.InApp,
            SentAt = channel != NotificationChannel.InApp ? DateTime.UtcNow : null
        };
        await unitOfWork.Repository<Notification>().AddAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SendEventNotificationAsync(string userId, NotificationEventType eventType, Dictionary<string, string>? placeholders = null, CancellationToken cancellationToken = default)
    {
        var template = await db.NotificationTemplates
            .FirstOrDefaultAsync(t => t.EventType == eventType && t.Channel == NotificationChannel.InApp && t.IsActive, cancellationToken);

        var title = template?.Subject ?? eventType.ToString();
        var message = template?.BodyTemplate ?? $"Notification for {eventType}";

        if (placeholders is not null)
        {
            foreach (var (key, value) in placeholders)
                message = message.Replace($"{{{key}}}", value);
        }

        await SendAsync(userId, eventType, NotificationChannel.InApp, title, message, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.EventType.ToString(), n.IsRead, n.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await db.Notifications.FindAsync([notificationId], cancellationToken);
        if (notification is not null)
        {
            notification.IsRead = true;
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
