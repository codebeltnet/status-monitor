using System;
using System.Collections.Generic;
using Cuemon;
using Cuemon.Extensions.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Codebelt.Shared
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureConfigurationSources(this IHostBuilder hostBuilder, Action<IHostEnvironment, IList<IConfigurationSource>> configureDelegate)
        {
            Validator.ThrowIfNull(hostBuilder);
            Validator.ThrowIfNull(configureDelegate);
            
            hostBuilder.ConfigureAppConfiguration((context, builder) =>
            {
                configureDelegate(context.HostingEnvironment, builder.Sources);
                if (context.HostingEnvironment.IsDevelopment())
                {
                    //builder.Sources.Remove(source => source is EnvironmentVariablesConfigurationSource);
                }
            });
            
            return hostBuilder;
        }

        public static IHostBuilder RemoveConfigurationSource(this IHostBuilder hostBuilder, Func<IHostEnvironment, IConfigurationSource, bool> predicate)
        {
            hostBuilder.ConfigureConfigurationSources((environment, sources) =>
            {
                sources.Remove(source => predicate(environment, source));
            });
            return hostBuilder;
        }
    }
}
