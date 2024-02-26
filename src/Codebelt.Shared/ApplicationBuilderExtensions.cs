using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Savvyio;

namespace Codebelt.Shared
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseHandlerServicesDescriptor(this IApplicationBuilder app)
        {
            var hsd = app.ApplicationServices.GetRequiredService<HandlerServicesDescriptor>();
            var logger = app.ApplicationServices.GetRequiredService<ILogger<HandlerServicesDescriptor>>();
            logger.LogInformation(hsd.ToString());
        }
    }
}
