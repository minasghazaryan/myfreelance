namespace MyFreelance.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(string? userId, string? adminUserId, Domain.Enums.AuditAction action, string entityType, string? entityId, string description, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);
}
