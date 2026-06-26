namespace MyFreelance.Application.DTOs.Notifications;

public record NotificationDto(Guid Id, string Title, string Message, string EventType, bool IsRead, DateTime CreatedAt);
