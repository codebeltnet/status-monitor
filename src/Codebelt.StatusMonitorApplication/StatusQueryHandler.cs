using System;
using System.Linq;
using Codebelt.StatusMonitor;
using Codebelt.StatusMonitorApplication.Queries;
using Codebelt.StatusMonitorApplication.Views;
using Savvyio.Handlers;
using Savvyio.Queries;

namespace Codebelt.StatusMonitorApplication
{
    public class StatusQueryHandler : QueryHandler
    {
        private readonly IStatusMonitorDataStore _statusMonitorDataStore;

        public StatusQueryHandler(IStatusMonitorDataStore statusMonitorDataStore)
        {
            _statusMonitorDataStore = statusMonitorDataStore;
        }

        protected override void RegisterDelegates(IRequestReplyRegistry<IQuery> handlers)
        {
            handlers.RegisterAsync<GetStatusQuery, StatusViewModel>(async query =>
            {
                var operation = (await _statusMonitorDataStore.FindAllAsync(options => options.Filter = operation => operation.Id.Equals(query.CorrelationId, StringComparison.OrdinalIgnoreCase) &&
                                                                                              operation.TenantId == query.TenantId).ConfigureAwait(false)).SingleOrDefault();
                return operation != null
                    ? new StatusViewModel(operation.Id, operation.Message, operation.Endpoint, operation.Scope, operation.Status)
                    {
                        AcceptedAt = operation.AcceptedAt,
                        RunningAt = operation.RunningAt,
                        SucceededAt = operation.SucceededAt,
                        FailedAt = operation.FailedAt,
                        FailedReason = operation.FailedReason,
                        EndpointRouteValue = operation.EndpointRouteValue
                    }
                    : null;
            });
        }
    }
}
