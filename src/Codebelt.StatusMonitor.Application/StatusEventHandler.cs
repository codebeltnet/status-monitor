using System.Threading.Tasks;
using Codebelt.StatusMonitor.Application.Events;
using Savvyio;
using Savvyio.EventDriven;
using Savvyio.EventDriven.Messaging;
using Savvyio.Extensions.DependencyInjection.Messaging;
using Savvyio.Extensions.SimpleQueueService.EventDriven;
using Savvyio.Handlers;

namespace Codebelt.StatusMonitor.Application
{
    public class StatusEventHandler : IntegrationEventHandler
    {
        private readonly IPublishSubscribeChannel<IIntegrationEvent, StatusEventHandler>? _eventBus;

        public StatusEventHandler(IPublishSubscribeChannel<IIntegrationEvent, StatusEventHandler>? eventBus = null)
        {
            _eventBus = eventBus;
        }

        protected override void RegisterDelegates(IFireForgetRegistry<IIntegrationEvent> handlers)
        {
            handlers.RegisterAsync<StatusAcceptedEvent>(@event =>
            {
                return _eventBus?.PublishAsync(@event.ToMessage("generic-status-monitor-topic.fifo".ToSnsUri(), "status.accepted")) ?? Task.CompletedTask;
            });
            handlers.RegisterAsync<StatusRunningEvent>(@event =>
            {
                return _eventBus?.PublishAsync(@event.ToMessage("generic-status-monitor-topic.fifo".ToSnsUri(), "status.running")) ?? Task.CompletedTask;
            });
            handlers.RegisterAsync<StatusDeletedEvent>(@event =>
            {
                return _eventBus?.PublishAsync(@event.ToMessage("generic-status-monitor-topic.fifo".ToSnsUri(), "status.deleted")) ?? Task.CompletedTask;
            });
            handlers.RegisterAsync<StatusSucceededEvent>(@event =>
            {
                return _eventBus?.PublishAsync(@event.ToMessage("generic-status-monitor-topic.fifo".ToSnsUri(), "status.succeeded")) ?? Task.CompletedTask;
            });
            handlers.RegisterAsync<StatusFailedEvent>(@event =>
            {
                return _eventBus?.PublishAsync(@event.ToMessage("generic-status-monitor-topic.fifo".ToSnsUri(), "status.failed")) ?? Task.CompletedTask;
            });
        }
    }
}
