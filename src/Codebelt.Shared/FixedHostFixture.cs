using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cuemon;
using Cuemon.Extensions.Xunit;
using Cuemon.Extensions.Xunit.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Codebelt.Shared
{
    public class FixedHostFixture : HostFixture
    {
        public override void ConfigureHost(Test hostTest)
        {
            var hostTestType = hostTest?.GetType();
            Validator.ThrowIfNull(hostTest);
            Validator.ThrowIfNotContainsType(hostTestType, nameof(hostTestType), $"{nameof(hostTest)} is not assignable from HostTest<T>.", typeof(HostTest<>));

            var hb = new HostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseEnvironment("Development")
                .ConfigureAppConfiguration((context, config) =>
                {
                    config
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                        .AddEnvironmentVariables();

                    ConfigureCallback(config.Build(), context.HostingEnvironment);
                })
                .ConfigureServices((context, services) =>
                {
                    Configuration = context.Configuration;
                    HostingEnvironment = context.HostingEnvironment;
                    ConfigureServicesCallback(services);
                });

            ConfigureHostCallback(hb);
            
            Host = hb.Build();
        }
    }
}
