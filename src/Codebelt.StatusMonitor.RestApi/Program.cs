using System.Threading.Tasks;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Codebelt.Bootstrapper.Web;
using Cuemon.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;

namespace Codebelt.StatusMonitor.RestApi
{
    public class Program : WebProgram<Startup>
    {
        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args)
                .RemoveConfigurationSource((environment, source) => environment.IsLocalDevelopment() && source is EnvironmentVariablesConfigurationSource)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddSystemsManager(o =>
                    {
                        var config = builder.Build();
                        o.Path = $"/{context.HostingEnvironment.EnvironmentName}/{context.HostingEnvironment.ApplicationName}/";
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
                .ConfigureLogging(builder =>
                {
                    builder.AddOpenTelemetry(o =>
                    {
                        o.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(nameof(StatusMonitor)));
                    });
                })
                .UseConsoleLifetime()
                .Build()
                .RunAsync();
        }
    }
}
