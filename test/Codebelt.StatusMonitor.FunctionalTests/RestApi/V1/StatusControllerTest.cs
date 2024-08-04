using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Codebelt.StatusMonitor.Application.Inputs;
using Codebelt.StatusMonitor.Application.Views;
using Cuemon.Extensions.IO;
using Cuemon.Extensions.Text.Json.Formatters;
using Cuemon.Extensions.Xunit;
using Cuemon.Extensions.Xunit.Hosting;
using Cuemon.Extensions.Xunit.Hosting.AspNetCore;
using Cuemon.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.Priority;

namespace Codebelt.StatusMonitor.RestApi.V1
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class StatusControllerTest : AspNetCoreHostTest<AspNetCoreHostFixture>
    {
        private Startup _startup;
        private HttpClient _client;
        private static readonly string CorrelationId = Guid.NewGuid().ToString("N");

        public StatusControllerTest(AspNetCoreHostFixture fixture, ITestOutputHelper output) : base(fixture, output)
        {
            _client = fixture.Host.GetTestClient();
            _client.DefaultRequestHeaders.Add(HttpHeaderNames.XApiKey, "705424692fda4b86b8726d64b22cb1bf");
            _client.Timeout = TimeSpan.FromSeconds(15);
        }

        [Fact, Priority(0)]
        public async Task Post_ShouldCreateNewStatus()
        {
            var inputModel = new CreateStatusInputModel()
            {
                CorrelationId = CorrelationId,
                Endpoint = "http://localhost/",
                Message = "Creating a new monitor.",
                Scope = OperationScope.Create
            };
            var response = await _client.PostAsync("/status", new StringContent(JsonFormatter.SerializeObject(inputModel).ToEncodedString(), Encoding.UTF8, "application/json"));
            var body = JsonFormatter.DeserializeObject<StatusViewModel>(await response.Content.ReadAsStreamAsync());

            TestOutput.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(inputModel.CorrelationId, body.CorrelationId);
            Assert.Equal(inputModel.Message, body.Message);
            Assert.Equal(inputModel.Endpoint, body.Endpoint);
            Assert.Equal(inputModel.Scope.ToString(), body.Scope);
            Assert.Equal(Status.Accepted, body.Status);
            Assert.InRange(body.AcceptedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
            Assert.Null(body.RunningAt);
            Assert.Null(body.FailedAt);
            Assert.Null(body.SucceededAt);
        }

        [Fact, Priority(1)]
        public async Task Get_ShouldGetStatus()
        {
            var response = await _client.GetAsync($"/status/{CorrelationId}");
            var body = JsonFormatter.DeserializeObject<StatusViewModel>(await response.Content.ReadAsStreamAsync());

            TestOutput.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(CorrelationId, body.CorrelationId);
            Assert.Equal("Creating a new monitor.", body.Message);
            Assert.Equal("http://localhost/", body.Endpoint);
            Assert.Equal(OperationScope.Create.ToString(), body.Scope);
            Assert.Equal(Status.Accepted, body.Status);
            Assert.InRange(body.AcceptedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
            Assert.Null(body.RunningAt);
            Assert.Null(body.FailedAt);
            Assert.Null(body.SucceededAt);
        }

        [Fact, Priority(2)]
        public async Task Patch_ShouldUpdateStatusToRunning()
        {
            var updateModel = new UpdateStatusInputModel()
            {
                RunningAt = DateTime.UtcNow,
                Message = "Running like the wind ..."
            };
            var response = await _client.PatchAsync($"/status/{CorrelationId}?status={OperationStatus.Running.ToString().ToLowerInvariant()}", new StringContent(JsonFormatter.SerializeObject(updateModel).ToEncodedString(), Encoding.UTF8, "application/json"));
            var body = JsonFormatter.DeserializeObject<StatusViewModel>(await response.Content.ReadAsStreamAsync());

            TestOutput.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(CorrelationId, body.CorrelationId);
            Assert.Equal(updateModel.Message, body.Message);
            Assert.Equal("http://localhost/", body.Endpoint);
            Assert.Equal(OperationScope.Create.ToString(), body.Scope);
            Assert.Equal(OperationStatus.Running.ToString(), body.Status);
            Assert.InRange(body.AcceptedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
            Assert.InRange(body.RunningAt!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        }

        [Fact, Priority(3)]
        public async Task Patch_ShouldUpdateStatusToSucceeded()
        {
            var updateModel = new UpdateStatusInputModel()
            {
                EndpointRouteValue = "all-is-good"
            };
            var response = await _client.PatchAsync($"/status/{CorrelationId}?status={OperationStatus.Succeeded.ToString().ToLowerInvariant()}", new StringContent(JsonFormatter.SerializeObject(updateModel).ToEncodedString(), Encoding.UTF8, "application/json"));
            var body = JsonFormatter.DeserializeObject<StatusViewModel>(await response.Content.ReadAsStreamAsync());

            TestOutput.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(CorrelationId, body.CorrelationId);
            Assert.Equal("Running like the wind ...", body.Message);
            Assert.Equal("http://localhost/", body.Endpoint);
            Assert.Equal(updateModel.EndpointRouteValue, body.EndpointRouteValue);
            Assert.Equal(OperationScope.Create.ToString(), body.Scope);
            Assert.Equal(OperationStatus.Succeeded.ToString(), body.Status);
            Assert.InRange(body.AcceptedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
            Assert.InRange(body.RunningAt!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
            Assert.InRange(body.SucceededAt!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        }

        [Fact, Priority(4)]
        public async Task Get_ShouldReceiveA303Redirection()
        {
            var response = await _client.GetAsync($"/status/{CorrelationId}");
            Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
            Assert.Equal("http://localhost/all-is-good", response.Headers.Location!.OriginalString);
        }

        [Fact, Priority(5)]
        public async Task Delete_ShouldDeleteStatus()
        {
            var response = await _client.DeleteAsync($"/status/{CorrelationId}");
            var body = await response.Content.ReadAsStreamAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(0, body.Length);
        }
        
        [Fact, Priority(6)]
        public async Task Get_ShouldReceiveA404NotFound()
        {
            var response = await _client.GetAsync($"/status/{CorrelationId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        protected override void ConfigureHost(IHostBuilder hb)
        {
            hb.UseEnvironment("LocalDevelopment");
        }

        public override void Configure(IConfiguration configuration, IHostEnvironment environment)
        {
            _startup = new Startup(configuration, environment);
            base.Configure(configuration, environment);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            _startup.ConfigureServices(services);
            services.AddXunitTestLogging(TestOutput, LogLevel.Information);
        }

        public override void ConfigureApplication(IApplicationBuilder app)
        {
            _startup.Configure(app, null);
        }
    }
}
