using System;
using Codebelt.StatusMonitor.Application.Commands;
using Savvyio.EventDriven;

namespace Codebelt.StatusMonitor.Application.Events
{
    public sealed record StatusAcceptedEvent : IntegrationEvent
    {
        public StatusAcceptedEvent(CreateStatusAcceptedCommand command)
        {
            CorrelationId = command.CorrelationId;
            Message = command.Message;
            Endpoint = command.Endpoint;
            Scope = command.Scope;
            Status = command.Status;
            AcceptedAt = command.AcceptedAt;
        }

        public string CorrelationId { get; }

        public string Message { get; }

        public string Endpoint { get; }
        
        public string Scope { get; }

        public string Status { get; }

        public DateTime AcceptedAt { get; }
    }
}
