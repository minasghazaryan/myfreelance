using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Domain.Interfaces;

namespace MyFreelance.Infrastructure.Services;

public class AuditService(IUnitOfWork unitOfWork) : IAuditService
{
    public async Task LogAsync(string? userId, string? adminUserId, AuditAction action, string entityType, string? entityId, string description, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            UserId = userId,
            AdminUserId = adminUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
        await unitOfWork.Repository<AuditLog>().AddAsync(log, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
