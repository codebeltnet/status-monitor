using System;
using Codebelt.StatusMonitor.Application.Commands;
using Savvyio.EventDriven;

namespace Codebelt.StatusMonitor.Application.Events
{
    public sealed record StatusRunningEvent : IntegrationEvent
    {
        public StatusRunningEvent(UpdateStatusToRunningCommand command)
        {
            CorrelationId = command.CorrelationId;
            Status = command.Status;
            Message = command.Message;
            RunningAt = command.RunningAt;
        }

        public string CorrelationId { get; }

        public string Status { get; }

        public string? Message { get; }

        public DateTime RunningAt { get; }
    }
}
