using Savvyio.Data;

namespace Codebelt.StatusMonitor
{
    public interface IStatusMonitorDataStore :  IWritableDataStore<Operation>, ISearchableDataStore<Operation, QueryOptions<Operation>>, IDeletableDataStore<Operation>
    {
    }
}
