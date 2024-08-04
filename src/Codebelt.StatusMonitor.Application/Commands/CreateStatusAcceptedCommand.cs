using System;
using Codebelt.SharedKernel;

namespace Codebelt.StatusMonitor.Application.Commands
{
    public sealed record CreateStatusAcceptedCommand : TenantCommand
    {
        public CreateStatusAcceptedCommand(TenantId tenantId, CorrelationId correlationId, Scope scope, Endpoint endpoint, Message message)
        {
            TenantId = tenantId;
            CorrelationId = correlationId;
            Message = message;
            Endpoint = endpoint;
            Scope = scope;
            Status = new Status(StatusMonitor.Status.Accepted);
            AcceptedAt = DateTime.UtcNow;
        }

        public string CorrelationId { get; }

        public string Message { get; }

        public string Endpoint { get; }
        
        public string Scope { get; }

        public string Status { get; }

        public DateTime AcceptedAt { get; }
    }
}
