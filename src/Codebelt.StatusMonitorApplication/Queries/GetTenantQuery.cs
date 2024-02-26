using Codebelt.StatusMonitorApplication.Views;
using Savvyio.Queries;

namespace Codebelt.StatusMonitorApplication.Queries
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
