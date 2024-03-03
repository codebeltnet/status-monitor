using System;
using Amazon;
using Amazon.Runtime;
using Codebelt.Bootstrapper.Worker;
using Codebelt.StatusMonitor.Application;
using Codebelt.StatusMonitor.InMemory;
using Cuemon.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Savvyio.Extensions;
using Savvyio.Extensions.DependencyInjection;
using Savvyio.Extensions.DependencyInjection.SimpleQueueService.Commands;
using Savvyio.Extensions.SimpleQueueService;
using Savvyio.Extensions.Text.Json;

namespace Codebelt.StatusMonitor.Worker
{
    public class Startup : WorkerStartup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment) : base(configuration, environment)
        {
            AmazonResourceNameOptions.DefaultAccountId = configuration["AWS:CallerIdentity"];
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSavvyIO(o =>
            {
                o.EnableHandlerServicesDescriptor()
                    .UseAutomaticDispatcherDiscovery(true)
                    .UseAutomaticHandlerDiscovery(true)
                    .AddMediator<Mediator>();
            });

            services.Add<JsonMarshaller>(o => o.Lifetime = ServiceLifetime.Singleton);

            services.ConfigureTriple<AmazonCommandQueueOptions<StatusCommandHandler>>(o =>
            {
                o.Credentials = new BasicAWSCredentials(Configuration["AWS:IAM:AccessKey"], Configuration["AWS:IAM:SecretKey"]);
                o.Endpoint = RegionEndpoint.EUWest1;
                o.SourceQueue = new Uri($"{Configuration["AWS:SourceQueue"]}/{Configuration["AWS:CallerIdentity"]}/generic-status-monitor.fifo");
                o.ConfigureClient(client =>
                {
                    client.ServiceURL = "http://localhost:4566";
                    client.AuthenticationRegion = RegionEndpoint.EUWest1.SystemName;
                });
            });
            services.Add<AmazonCommandQueue<StatusCommandHandler>>(o => o.Lifetime = ServiceLifetime.Singleton);

            services.Add<InMemoryTenantDataStore>(o => o.Lifetime = ServiceLifetime.Singleton);
            services.Add<InMemoryStatusMonitorDataStore>(o => o.Lifetime = ServiceLifetime.Singleton);

            services.AddHostedService<Worker>();
        }
    }
}
