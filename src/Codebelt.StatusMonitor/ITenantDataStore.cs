using Savvyio.Data;

namespace Codebelt.StatusMonitor
{
    public interface ITenantDataStore : ISearchableDataStore<Tenant, QueryOptions<Tenant>>
    {
    }
}
