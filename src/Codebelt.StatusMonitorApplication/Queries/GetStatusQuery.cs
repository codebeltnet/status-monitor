using System;
using Codebelt.StatusMonitor;
using Codebelt.StatusMonitorApplication.Views;
using Savvyio.Queries;

namespace Codebelt.StatusMonitorApplication.Queries
{
	public record GetStatusQuery : Query<StatusViewModel>
	{
		public GetStatusQuery(TenantId tenantId, CorrelationId correlationId)
		{
			TenantId = tenantId;
            CorrelationId = correlationId;
        }

		public Guid TenantId { get; }

		public string CorrelationId { get; }
	}
}
