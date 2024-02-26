using System;
using System.Linq;
using Codebelt.StatusMonitor;
using Codebelt.StatusMonitorApplication.Queries;
using Codebelt.StatusMonitorApplication.Views;
using Savvyio.Handlers;
using Savvyio.Queries;

namespace Codebelt.StatusMonitorApplication
{
    public class TenantQueryHandler : QueryHandler
    {
        private readonly ITenantDataStore _tenantDataStore;

        public TenantQueryHandler(ITenantDataStore tenantDataStore)
        {
            _tenantDataStore = tenantDataStore;
        }

        protected override void RegisterDelegates(IRequestReplyRegistry<IQuery> handlers)
        {
            handlers.RegisterAsync<GetTenantQuery, TenantViewModel?>(async query =>
            {
                var tenant = (await _tenantDataStore.FindAllAsync(options =>
                {
                    options.MaxInclusiveResultCount = 1;
                    options.Filter = tenant => tenant.AccessKeys.Any(key => key.Secret.Equals(query.ApiKey, StringComparison.OrdinalIgnoreCase) &&
                                                                            key.Enabled &&
                                                                            key.Expires > DateTime.UtcNow);
                }).ConfigureAwait(false)).SingleOrDefault();
                return tenant != null
                    ? new TenantViewModel() { TenantId = tenant.Id }
                    : null;
            });
        }
    }
}
