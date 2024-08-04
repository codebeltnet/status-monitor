using System;
using Amazon;
using Amazon.Runtime;
using Codebelt.Bootstrapper.Worker;
using Codebelt.StatusMonitor.Application;
using Codebelt.StatusMonitor.InMemory;
using Cuemon.Extensions.DependencyInjection;
using Cuemon.Extensions.Hosting;
using Cuemon.Extensions.Text.Json.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Savvyio.Extensions;
using Savvyio.Extensions.DependencyInjection;
using Savvyio.Extensions.DependencyInjection.SimpleQueueService.Commands;
using Savvyio.Extensions.DependencyInjection.SimpleQueueService.EventDriven;
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

            services.AddConfiguredOptions<JsonFormatterOptions>(o => o.Settings.WriteIndented = false);
            services.AddMarshaller<JsonMarshaller>(o => o.Lifetime = ServiceLifetime.Singleton);

            services.AddConfiguredOptions<AmazonCommandQueueOptions<StatusCommandHandler>>(o =>
            {
                o.Credentials = new BasicAWSCredentials(Configuration["AWS:IAM:AccessKey"], Configuration["AWS:IAM:SecretKey"]);
                o.Endpoint = RegionEndpoint.GetBySystemName(Configuration["AWS:Region"]);
                o.SourceQueue = new Uri($"{Configuration["AWS:SourceQueue"]}/{Configuration["AWS:CallerIdentity"]}/status-monitor.fifo");
                if (Environment.IsLocalDevelopment())
                {
                    o.ReceiveContext.PollingTimeout = TimeSpan.FromSeconds(1);
                    o.ConfigureClient(client =>
                    {
                        client.ServiceURL = Configuration["AWS:ServiceUrl"];
                        client.AuthenticationRegion = Configuration["AWS:Region"];
                    });
                }
            });
            services.Add<AmazonCommandQueue<StatusCommandHandler>>(o => o.Lifetime = ServiceLifetime.Singleton);

            services.AddConfiguredOptions<AmazonEventBusOptions<StatusEventHandler>>(o =>
            {
                o.Credentials = new BasicAWSCredentials(Configuration["AWS:IAM:AccessKey"], Configuration["AWS:IAM:SecretKey"]);
                o.Endpoint = RegionEndpoint.GetBySystemName(Configuration["AWS:Region"]);
                o.SourceQueue = new Uri($"{Configuration["AWS:SourceQueue"]}/{Configuration["AWS:CallerIdentity"]}/status-monitor-events.fifo");
                if (Environment.IsLocalDevelopment())
                {
                    o.ReceiveContext.PollingTimeout = TimeSpan.FromSeconds(1);
                    o.ConfigureClient(client =>
                    {
                        client.ServiceURL = Configuration["AWS:ServiceUrl"];
                        client.AuthenticationRegion = Configuration["AWS:Region"];
                    });
                }
            });
            services.Add<AmazonEventBus<StatusEventHandler>>(o => o.Lifetime = ServiceLifetime.Singleton);

            if (Environment.IsLocalDevelopment())
            {
                services.Add<InMemoryTenantDataStore>(o => o.Lifetime = ServiceLifetime.Singleton);
                services.Add<InMemoryStatusMonitorDataStore>(o => o.Lifetime = ServiceLifetime.Singleton);
            }

            services.AddHostedService<QueueWorker>();
        }
    }
}
