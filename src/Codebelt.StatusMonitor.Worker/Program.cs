using System.Threading.Tasks;
using Amazon;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Codebelt.Bootstrapper.Worker;
using Codebelt.Shared;
using Cuemon.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Hosting;

namespace Codebelt.StatusMonitor.Worker
{
    public class Program : WorkerProgram<Startup>
    {
        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args)
                //.ConfigureAppConfiguration(builder =>
                //{
                //    var config = builder.Build();
                //    builder.AddAzureKeyVault(new Uri($"https://{config["Azure:KeyVault:Name"]}.vault.azure.net/"), new DefaultAzureCredential());
                //})
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
                .Build()
                .RunAsync();
        }
    }
}
