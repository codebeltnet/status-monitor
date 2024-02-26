using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Codebelt.StatusMonitor;
using Cuemon.Extensions;

namespace Codebelt.StatusMonitorInMemory
{
    public class InMemoryTenantDataStore : ITenantDataStore
    {
        private readonly List<Tenant> _tenants = new()
        {
            new Tenant(new TenantId(Guid.Parse("13dd96d361a4455f8b1a1ad9b3f43dce")))
            {
                AccessKeys = new List<AccessKey>()
                {
                    new(new TenantId(Guid.Parse("13dd96d361a4455f8b1a1ad9b3f43dce")), "fecf3fe0ad14418db6a31fb6235aa760", DateTime.Parse("2024-01-01T00:00:00Z", CultureInfo.InvariantCulture)),
                    new(new TenantId(Guid.Parse("13dd96d361a4455f8b1a1ad9b3f43dce")), "705424692fda4b86b8726d64b22cb1bf")
                }
            }
        };

        public Task<IEnumerable<Tenant>> FindAllAsync(Action<QueryOptions<Tenant>> setup = null)
        {
            var options = setup.Configure();
            return Task.FromResult(_tenants.FindAll(operation => options.Filter(operation)).Take(options.MaxInclusiveResultCount));
        }
    }
}
