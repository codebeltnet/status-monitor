using System;
using Codebelt.StatusMonitor;
using Savvyio.Commands;

namespace Codebelt.StatusMonitorApplication.Commands
{
    public record AcceptedOperationCommand : Command
    {
        public AcceptedOperationCommand(TenantId tenantId, CorrelationId correlationId, Scope scope, Endpoint endpoint, Message message)
        {
            TenantId = tenantId;
            CorrelationId = correlationId;
            Message = message;
            Endpoint = endpoint;
            Scope = scope;
            Status = new Status(StatusMonitor.Status.Accepted);
            AcceptedAt = DateTime.UtcNow;
        }

        public Guid TenantId { get; }

        public string CorrelationId { get; }

        public string Message { get; }

        public string Endpoint { get; }
        
        public string Scope { get; }

        public string Status { get; }

        public DateTime AcceptedAt { get; }
    }
}
