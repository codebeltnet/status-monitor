using System.Threading.Tasks;
using Codebelt.Bootstrapper.Worker;
using Codebelt.Shared;
using Cuemon.Extensions.Hosting;
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
                .Build()
                .RunAsync();
        }
    }
}
