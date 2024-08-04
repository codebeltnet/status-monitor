using System;
using Codebelt.SharedKernel;
using Codebelt.StatusMonitor.Application.Views;
using Savvyio.Queries;

namespace Codebelt.StatusMonitor.Application.Queries
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
