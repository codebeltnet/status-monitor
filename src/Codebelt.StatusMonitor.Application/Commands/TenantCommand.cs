using System;
using Savvyio.Commands;

namespace Codebelt.StatusMonitor.Application.Commands
{
    public abstract record TenantCommand : Command
    {
        public Guid TenantId { get; protected set; }
    }
}
