using System;
using Codebelt.StatusMonitor;
using Savvyio.Commands;

namespace Codebelt.StatusMonitorApplication.Commands
{
    public record SucceededOperationCommand : Command
    {
        public SucceededOperationCommand(TenantId tenantId, CorrelationId correlationId, EndpointRouteValue? endpointRouteValue = null, CoordinatedUniversalTime? succeededAt = null)
        {
            TenantId = tenantId;
            CorrelationId = correlationId;
            Status = new Status(OperationStatus.Succeeded);
            SucceededAt = succeededAt ?? DateTime.UtcNow;
            if (endpointRouteValue != null) { EndpointRouteValue = endpointRouteValue; }
        }

        public Guid TenantId { get; }

        public string CorrelationId { get; }

        public string Status { get; }

        public string? EndpointRouteValue { get; }

        public DateTime SucceededAt { get; }
    }
}
