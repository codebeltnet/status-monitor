﻿using System;
using System.Threading;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Cuemon;
using Cuemon.Extensions.Hosting;
using Cuemon.Extensions.Xunit.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Codebelt.StatusMonitor.Worker
{
    public class WorkerFixture : Disposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        public WorkerFixture()
        {
            CancellationToken = _cancellationTokenSource.Token;
            Host.CreateDefaultBuilder()
                .UseEnvironment("LocalDevelopment")
                .RemoveConfigurationSource((_, source) => source is EnvironmentVariablesConfigurationSource)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddSystemsManager(o =>
                    {
                        var config = builder.Build();
                        o.Path = $"/{context.HostingEnvironment.EnvironmentName}/Codebelt.StatusMonitor.Worker/";
                        if (context.HostingEnvironment.IsLocalDevelopment())
                        {
                            o.AwsOptions = new AWSOptions()
                            {
                                DefaultClientConfig =
                                {
                                    ServiceURL = config["AWS:ServiceUrl"],
                                    AuthenticationRegion = config["AWS:Region"]
                                },
                                Credentials = new BasicAWSCredentials(config["AWS:IAM:AccessKey"], config["AWS:IAM:SecretKey"])
                            };
                        }
                        else
                        {
                            o.AwsOptions = new AWSOptions()
                            {
                                DefaultClientConfig =
                                {
                                    RegionEndpoint = RegionEndpoint.GetBySystemName(config["AWS:Region"])
                                },
                                Credentials = new BasicAWSCredentials(config["AWS:IAM:AccessKey"], config["AWS:IAM:SecretKey"])
                            };
                        }
                    });
                })
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
