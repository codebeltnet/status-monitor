using System;
using Codebelt.SharedKernel;

namespace Codebelt.StatusMonitor.Application.Commands
{
    public sealed record UpdateStatusToRunningCommand : TenantCommand
    {
        public UpdateStatusToRunningCommand(TenantId tenantId, CorrelationId correlationId, Message? message = null, CoordinatedUniversalTime? runningAt = null)
        {
            TenantId = tenantId;
            CorrelationId = correlationId;
            Status = new Status(OperationStatus.Running);
            RunningAt = runningAt ?? DateTime.UtcNow;
            if (message != null) { Message = message; }
        }

        public string CorrelationId { get; }

        public string Status { get; }

        public string? Message { get; }

        public DateTime RunningAt { get; }
    }
}
