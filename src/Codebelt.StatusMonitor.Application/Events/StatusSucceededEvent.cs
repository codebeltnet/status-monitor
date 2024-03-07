using System;
using Codebelt.StatusMonitor.Application.Commands;
using Savvyio.EventDriven;

namespace Codebelt.StatusMonitor.Application.Events
{
    public sealed record StatusSucceededEvent : IntegrationEvent
    {
        public StatusSucceededEvent(UpdateStatusToSucceededCommand command)
        {
            CorrelationId = command.CorrelationId;
            Status = command.Status;
            EndpointRouteValue = command.EndpointRouteValue;
            SucceededAt = command.SucceededAt;
        }

        public string CorrelationId { get; }

        public string Status { get; }

        public string? EndpointRouteValue { get; }

        public DateTime SucceededAt { get; }
    }
}
