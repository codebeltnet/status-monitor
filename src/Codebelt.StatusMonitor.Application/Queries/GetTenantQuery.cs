using Codebelt.StatusMonitor.Application.Views;
using Savvyio.Queries;

namespace Codebelt.StatusMonitor.Application.Queries
{
    public record GetTenantQuery : Query<TenantViewModel>
    {
        public GetTenantQuery(string apiKey)
        {
            ApiKey = apiKey;
        }

        public string ApiKey { get; }
    }
}
