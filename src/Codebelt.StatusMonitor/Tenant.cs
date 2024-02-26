using System;
using System.Collections.Generic;
using Savvyio.Domain;

namespace Codebelt.StatusMonitor
{
    public class Tenant : Entity<Guid>
    {
        Tenant()
        {
        }

        public Tenant(TenantId id) : base(id.Value)
        {
        }

        public IEnumerable<AccessKey> AccessKeys { get; set; }
    }
}
