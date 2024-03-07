using System;

namespace Codebelt.StatusMonitor.Application.Commands
{
    public sealed record UpdateStatusToFailedCommand : TenantCommand
    {
        public UpdateStatusToFailedCommand(TenantId tenantId, CorrelationId correlationId, FailedReason failedReason, CoordinatedUniversalTime? failedAt = null)
        {
            TenantId = tenantId;
            CorrelationId = correlationId;
            Status = new Status(OperationStatus.Failed);
            FailedReason = failedReason;
            FailedAt = failedAt ?? DateTime.UtcNow;
        }

        public string CorrelationId { get; }

        public string Status { get; }

        public string FailedReason { get; }

        public DateTime FailedAt { get; }
    }
}
