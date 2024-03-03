using System;
using Savvyio.Commands;

namespace Codebelt.StatusMonitor.Application.Commands
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
