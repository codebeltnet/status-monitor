using System;
using Savvyio.Commands;

namespace Codebelt.StatusMonitor.Application.Commands
{
    public record RunningOperationCommand : Command
    {
        public RunningOperationCommand(TenantId tenantId, CorrelationId correlationId, Message? message = null, CoordinatedUniversalTime? runningAt = null)
        {
            TenantId = tenantId;
            CorrelationId = correlationId;
            Status = new Status(OperationStatus.Running);
            RunningAt = runningAt ?? DateTime.UtcNow;
            if (message != null) { Message = message; }
        }

        public Guid TenantId { get; }

        public string CorrelationId { get; }

        public string Status { get; }

        public string? Message { get; }

        public DateTime RunningAt { get; }
    }
}
