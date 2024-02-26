using System;
using Codebelt.StatusMonitor;
using Savvyio.Commands;

namespace Codebelt.StatusMonitorApplication.Commands
{
    public record DeleteOperationCommand : Command
    {
        public DeleteOperationCommand(TenantId tenantId, CorrelationId correlationId)
        {
            TenantId = tenantId;
            CorrelationId = correlationId;
        }

        public Guid TenantId { get; }

        public string CorrelationId { get; }
    }
}
