using System;
using Codebelt.SharedKernel;

namespace Codebelt.StatusMonitor.Application.Commands
{
    public sealed record UpdateStatusToSucceededCommand : TenantCommand
    {
        public UpdateStatusToSucceededCommand(TenantId tenantId, CorrelationId correlationId, EndpointRouteValue? endpointRouteValue = null, CoordinatedUniversalTime? succeededAt = null)
        {
            TenantId = tenantId;
            CorrelationId = correlationId;
            Status = new Status(OperationStatus.Succeeded);
            SucceededAt = succeededAt ?? DateTime.UtcNow;
            if (endpointRouteValue != null) { EndpointRouteValue = endpointRouteValue; }
        }

        public string CorrelationId { get; }

        public string Status { get; }

        public string? EndpointRouteValue { get; }

        public DateTime SucceededAt { get; }
    }
}
