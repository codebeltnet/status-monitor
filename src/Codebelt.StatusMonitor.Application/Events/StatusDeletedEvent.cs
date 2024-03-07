using Codebelt.StatusMonitor.Application.Commands;
using Savvyio.EventDriven;

namespace Codebelt.StatusMonitor.Application.Events
{
    public sealed record StatusDeletedEvent : IntegrationEvent
    {
        public StatusDeletedEvent(DeleteStatusCommand command)
        {
            CorrelationId = command.CorrelationId;
        }

        public string CorrelationId { get; }
    }
}
