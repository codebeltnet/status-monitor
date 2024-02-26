using System;
using Codebelt.StatusMonitor;
using Savvyio.Commands;

namespace Codebelt.StatusMonitorApplication.Commands
{
    public record FailedOperationCommand : Command
    {
        public FailedOperationCommand(TenantId tenantId, CorrelationId correlationId, FailedReason failedReason, CoordinatedUniversalTime? failedAt = null)
        {
            TenantId = tenantId;
            CorrelationId = correlationId;
            Status = new Status(OperationStatus.Failed);
            FailedReason = failedReason;
            FailedAt = failedAt ?? DateTime.UtcNow;
        }

        public Guid TenantId { get; }

        public string CorrelationId { get; }

        public string Status { get; }

        public string FailedReason { get; }

        public DateTime FailedAt { get; }
    }
}
