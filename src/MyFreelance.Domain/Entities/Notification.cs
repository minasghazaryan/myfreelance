using MyFreelance.Domain.Common;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Domain.Entities;

public class Notification : BaseEntity
{
    public string? UserId { get; set; }
    public NotificationEventType EventType { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsSent { get; set; }

    public ApplicationUser? User { get; set; }
}
