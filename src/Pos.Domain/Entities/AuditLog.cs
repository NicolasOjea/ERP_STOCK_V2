using Pos.Domain.Common;
using Pos.Domain.Enums;

namespace Pos.Domain.Entities;

public sealed class AuditLog : EntityBase
{
    public AuditLog(
        Guid id,
        Guid tenantId,
        Guid? userId,
        AuditAction action,
        string entityName,
        string entityId,
        DateTimeOffset occurredAtUtc,
        string? detail)
        : base(id, tenantId, occurredAtUtc)
    {
        UserId = userId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        OccurredAt = occurredAtUtc;
        Detail = detail;
    }

    public Guid? UserId { get; private set; }
    public AuditAction Action { get; private set; }
    public string EntityName { get; private set; }
    public string EntityId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string? Detail { get; private set; }
}
