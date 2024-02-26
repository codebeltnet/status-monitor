using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Savvyio;

namespace Codebelt.Shared
{
    public static class ServiceProviderExtensions
    {
        public static void UseHandlerServicesDescriptor(this IServiceProvider provider)
        {
            var hsd = provider.GetRequiredService<HandlerServicesDescriptor>();
            var logger = provider.GetRequiredService<ILogger<HandlerServicesDescriptor>>();
            logger.LogInformation(hsd.ToString());
        }
    }
}
