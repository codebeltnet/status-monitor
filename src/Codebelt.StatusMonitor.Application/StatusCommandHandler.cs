using System;
using System.Linq;
using System.Threading.Tasks;
using Codebelt.StatusMonitor.Application.Commands;
using Savvyio.Commands;
using Savvyio.Handlers;

namespace Codebelt.StatusMonitor.Application
{
    public class StatusCommandHandler : CommandHandler
    {
        private readonly IStatusMonitorDataStore _statusMonitorDataStore;

        public StatusCommandHandler(IStatusMonitorDataStore statusMonitorDataStore)
        {
            _statusMonitorDataStore = statusMonitorDataStore;
        }

        protected override void RegisterDelegates(IFireForgetRegistry<ICommand> handlers)
        {
            handlers.RegisterAsync<AcceptedOperationCommand>(command =>
                _statusMonitorDataStore.CreateAsync(new Operation(command.TenantId, command.CorrelationId)
                    .Accepted(command.Scope, command.Endpoint, command.Message, command.AcceptedAt)));
            handlers.RegisterAsync<DeleteOperationCommand>(command =>
                _statusMonitorDataStore.DeleteAsync(new Operation(command.TenantId, command.CorrelationId)));
            handlers.RegisterAsync<FailedOperationCommand>(async command =>
            {
                var operation = await GetOperationAsync(command.CorrelationId, command.TenantId).ConfigureAwait(false);
                if (operation != null)
                {
                    await _statusMonitorDataStore.UpdateAsync(operation.Failed(command.FailedReason, command.FailedAt)).ConfigureAwait(false);
                }
            });
            handlers.RegisterAsync<RunningOperationCommand>(async command =>
            {
                var operation = await GetOperationAsync(command.CorrelationId, command.TenantId).ConfigureAwait(false);
                if (operation != null)
                {
                    await _statusMonitorDataStore.UpdateAsync(operation.Running(command.Message, command.RunningAt)).ConfigureAwait(false);
                }
                
            });
            handlers.RegisterAsync<SucceededOperationCommand>(async command =>
            {
                var operation = await GetOperationAsync(command.CorrelationId, command.TenantId).ConfigureAwait(false);
                if (operation != null)
                {
                    await _statusMonitorDataStore.UpdateAsync(operation.Succeeded(command.EndpointRouteValue, command.SucceededAt)).ConfigureAwait(false);
                }
            });
        }

        private async Task<Operation?> GetOperationAsync(string correlationId, Guid tenantId)
        {
            return (await _statusMonitorDataStore.FindAllAsync(o => o.Filter = operation => operation.Id == correlationId && operation.TenantId == tenantId).ConfigureAwait(false)).SingleOrDefault();
        }
    }
}
