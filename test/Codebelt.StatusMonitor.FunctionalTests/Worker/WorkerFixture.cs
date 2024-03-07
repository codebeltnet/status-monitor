using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cuemon;
using Cuemon.Extensions.Hosting;
using Cuemon.Extensions.Xunit.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Codebelt.StatusMonitor.Worker
{
    public class WorkerFixture : Disposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        private IGenericHostTest _instance;

        public WorkerFixture()
        {
            CancellationToken = _cancellationTokenSource.Token;
            Host.CreateDefaultBuilder()
                .UseEnvironment("LocalDevelopment")
                .RemoveConfigurationSource((_, source) => source is EnvironmentVariablesConfigurationSource)
                .ConfigureServices((context, _) =>
                {
                    Configuration = context.Configuration;
                    Environment = context.HostingEnvironment;

                    var startup = new Startup(context.Configuration, context.HostingEnvironment);
                    var instance = GenericHostTestFactory.Create(services =>
                    {
                        ConfigureServicesCallback?.Invoke(services);
                        startup.ConfigureServices(services);
                    });
                    var hostedService = instance.ServiceProvider.GetRequiredService<IHostedService>();
                    hostedService.StartAsync(CancellationToken).GetAwaiter().GetResult();
                    HostTest = instance;
                })
                .Build();
            CorrelationId = Guid.NewGuid().ToString("N");
        }

        public IGenericHostTest HostTest { get; private set; }

        public Action<IServiceCollection> ConfigureServicesCallback { get; set; }

        protected override void OnDisposeManagedResources()
        {
            _cancellationTokenSource?.Dispose();
        }

        public CancellationToken CancellationToken { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IHostEnvironment Environment { get; private set; }

        public string CorrelationId { get; }
    }
}
