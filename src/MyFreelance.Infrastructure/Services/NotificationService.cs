using System.Text.Json;
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
    public async Task SendAsync(string? userId, NotificationEventType eventType, NotificationChannel channel, string title, string message, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            EventType = eventType,
            Channel = channel,
            Title = title,
            Message = message,
            MetadataJson = metadata is null || metadata.Count == 0 ? null : JsonSerializer.Serialize(metadata),
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
            {
                title = title.Replace($"{{{key}}}", value);
                message = message.Replace($"{{{key}}}", value);
            }
        }

        await SendAsync(userId, eventType, NotificationChannel.InApp, title, message, placeholders, cancellationToken);
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

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }

    public async Task<NotificationDetailDto?> GetNotificationDetailAsync(string userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

        if (notification is null)
            return null;

        var metadata = ParseMetadata(notification.MetadataJson);
        var details = BuildDetailItems(notification.EventType, metadata);

        return new NotificationDetailDto(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.EventType.ToString(),
            notification.IsRead,
            notification.CreatedAt,
            details);
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

    private static Dictionary<string, string> ParseMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return [];

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static IReadOnlyList<NotificationDetailItemDto> BuildDetailItems(NotificationEventType eventType, Dictionary<string, string> metadata)
    {
        if (metadata.Count == 0)
            return [];

        string[] orderedKeys = eventType switch
        {
            NotificationEventType.ReferralReward =>
                ["Amount", "ReferralName", "ReferralEmail", "Level", "Percentage", "SourceAmount", "Status", "Description"],
            NotificationEventType.KycStatusChange =>
                ["Status", "Description", "RejectionReason"],
            NotificationEventType.Verification =>
                ["Status", "Description", "PhoneNumber"],
            NotificationEventType.Deposit or NotificationEventType.Withdrawal =>
                ["Amount", "Status", "Description", "TransactionHash"],
            NotificationEventType.TierUpgrade =>
                ["TierName", "Amount", "ProjectedYield", "Status", "Description"],
            _ =>
                ["Status", "Description", "Amount", "ReferralName", "TierName"]
        };

        var items = new List<NotificationDetailItemDto>();
        var usedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in orderedKeys)
        {
            if (!metadata.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value) || value == "—")
                continue;

            items.Add(new NotificationDetailItemDto(FormatLabel(key), value));
            usedKeys.Add(key);
        }

        foreach (var (key, value) in metadata)
        {
            if (usedKeys.Contains(key) || string.IsNullOrWhiteSpace(value))
                continue;

            items.Add(new NotificationDetailItemDto(FormatLabel(key), value));
        }

        return items;
    }

    private static string FormatLabel(string key) => key switch
    {
        "Amount" => "Amount",
        "ReferralName" => "From Referral",
        "ReferralEmail" => "Referral Email",
        "Level" => "Referral Level",
        "Percentage" => "Commission Rate",
        "SourceAmount" => "Source Deposit",
        "Status" => "Status",
        "Description" => "Description",
        "RejectionReason" => "Rejection Reason",
        "PhoneNumber" => "Phone Number",
        "TierName" => "Investment Tier",
        "ProjectedYield" => "Projected Yield",
        "TransactionHash" => "Transaction Hash",
        _ => key
    };
}
