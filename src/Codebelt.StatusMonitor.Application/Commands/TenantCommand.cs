using System;
using Savvyio;
using Savvyio.Commands;

namespace Codebelt.StatusMonitor.Application.Commands
{
    public abstract record TenantCommand : Command
    {
        protected TenantCommand()
        {
            Metadata.Remove(nameof(MetadataDictionary.CorrelationId));
        }

        public Guid TenantId { get; protected set; }
    }
}
