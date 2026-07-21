namespace MyFreelance.Application.DTOs.Notifications;

public record NotificationDto(Guid Id, string Title, string Message, string EventType, bool IsRead, DateTime CreatedAt);

public record NotificationDetailItemDto(string Label, string Value);

public record NotificationDetailDto(
    Guid Id,
    string Title,
    string Message,
    string EventType,
    bool IsRead,
    DateTime CreatedAt,
    IReadOnlyList<NotificationDetailItemDto> Details);
