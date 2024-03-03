using System.Threading.Tasks;
using Codebelt.Bootstrapper.Web;
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
