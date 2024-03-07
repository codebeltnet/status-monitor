using System;
using System.Linq;
using System.Threading.Tasks;
using Codebelt.StatusMonitor.Application.Commands;
using Codebelt.StatusMonitor.Application.Events;
using Savvyio;
using Savvyio.Commands;
using Savvyio.Extensions;
using Savvyio.Handlers;

namespace Codebelt.StatusMonitor.Application
{
    public class StatusCommandHandler : CommandHandler
    {
        private readonly IMediator _mediator;
        private readonly IStatusMonitorDataStore _statusMonitorDataStore;

        public StatusCommandHandler(IMediator mediator,  IStatusMonitorDataStore statusMonitorDataStore)
        {
            _mediator = mediator;
            _statusMonitorDataStore = statusMonitorDataStore;
        }

        protected override void RegisterDelegates(IFireForgetRegistry<ICommand> handlers)
        {
            handlers.RegisterAsync<CreateStatusAcceptedCommand>(async command =>
            {
                await _statusMonitorDataStore.CreateAsync(new Operation(command.TenantId, command.CorrelationId)
                    .Accepted(command.Scope, command.Endpoint, command.Message, command.AcceptedAt)).ConfigureAwait(false);
                await _mediator.PublishAsync(new StatusAcceptedEvent(command)
                    .SetCorrelationId(command.CorrelationId)).ConfigureAwait(false);
            });
            handlers.RegisterAsync<DeleteStatusCommand>(async command =>
            {
                await _statusMonitorDataStore.DeleteAsync(new Operation(command.TenantId, command.CorrelationId)).ConfigureAwait(false);
                await _mediator.PublishAsync(new StatusDeletedEvent(command)
                    .SetCorrelationId(command.CorrelationId)).ConfigureAwait(false);
            });
            handlers.RegisterAsync<UpdateStatusToFailedCommand>(async command =>
            {
                var operation = await GetOperationAsync(command.CorrelationId, command.TenantId).ConfigureAwait(false);
                if (operation != null)
                {
                    await _statusMonitorDataStore.UpdateAsync(operation.Failed(command.FailedReason, command.FailedAt)).ConfigureAwait(false);
                    await _mediator.PublishAsync(new StatusFailedEvent(command)
                        .SetCorrelationId(command.CorrelationId)).ConfigureAwait(false);
                }
            });
            handlers.RegisterAsync<UpdateStatusToRunningCommand>(async command =>
            {
                var operation = await GetOperationAsync(command.CorrelationId, command.TenantId).ConfigureAwait(false);
                if (operation != null)
                {
                    await _statusMonitorDataStore.UpdateAsync(operation.Running(command.Message, command.RunningAt)).ConfigureAwait(false);
                    await _mediator.PublishAsync(new StatusRunningEvent(command)
                        .SetCorrelationId(command.CorrelationId)).ConfigureAwait(false);
                }
                
            });
            handlers.RegisterAsync<UpdateStatusToSucceededCommand>(async command =>
            {
                var operation = await GetOperationAsync(command.CorrelationId, command.TenantId).ConfigureAwait(false);
                if (operation != null)
                {
                    await _statusMonitorDataStore.UpdateAsync(operation.Succeeded(command.EndpointRouteValue, command.SucceededAt)).ConfigureAwait(false);
                    await _mediator.PublishAsync(new StatusSucceededEvent(command)
                        .SetCorrelationId(command.CorrelationId)).ConfigureAwait(false);
                }
            });
        }

        private async Task<Operation?> GetOperationAsync(string correlationId, Guid tenantId)
        {
            return (await _statusMonitorDataStore.FindAllAsync(o => o.Filter = operation => operation.Id == correlationId && operation.TenantId == tenantId).ConfigureAwait(false)).SingleOrDefault();
        }
    }
}
