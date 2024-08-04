using Codebelt.SharedKernel;
using Savvyio;

namespace Codebelt.StatusMonitor.Application.Commands
{
    public sealed record DeleteStatusCommand : TenantCommand
    {
        public DeleteStatusCommand(TenantId tenantId, CorrelationId correlationId)
        {
            TenantId = tenantId;
            CorrelationId = correlationId;
        }

        public string CorrelationId { get; }
    }
}
