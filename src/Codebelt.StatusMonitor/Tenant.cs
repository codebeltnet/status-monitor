using System;
using System.Collections.Generic;
using Codebelt.SharedKernel.Security;
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
