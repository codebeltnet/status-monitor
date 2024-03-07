using System;
using Codebelt.StatusMonitor.Application.Commands;
using Savvyio.EventDriven;

namespace Codebelt.StatusMonitor.Application.Events
{
    public sealed record StatusFailedEvent : IntegrationEvent
    {
        public StatusFailedEvent(UpdateStatusToFailedCommand command)
        {
            CorrelationId = command.CorrelationId;
            Status = command.Status;
            FailedReason = command.FailedReason;
            FailedAt = command.FailedAt;
        }

        public string CorrelationId { get; }

        public string Status { get; }

        public string FailedReason { get; }

        public DateTime FailedAt { get; }
    }
}
