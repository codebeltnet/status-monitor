using System;
using System.Threading;
using System.Threading.Tasks;
using Codebelt.Shared;
using Codebelt.StatusMonitor.Application;
using Codebelt.StatusMonitor.Application.Commands;
using Codebelt.StatusMonitor.Application.Events;
using Codebelt.StatusMonitor.Application.Queries;
using Cuemon;
using Cuemon.Extensions;
using Cuemon.Extensions.Collections.Generic;
using Cuemon.Extensions.IO;
using Cuemon.Extensions.Text.Json.Formatters;
using Cuemon.Extensions.Xunit;
using Cuemon.Extensions.Xunit.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Savvyio;
using Savvyio.Commands;
using Savvyio.Commands.Messaging;
using Savvyio.EventDriven;
using Savvyio.Extensions;
using Savvyio.Extensions.DependencyInjection.Messaging;
using Savvyio.Messaging.Cryptography;
using Xunit.Abstractions;
using Xunit.Priority;

namespace Codebelt.StatusMonitor.Worker
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class QueueWorkerTest : Test, IClassFixture<WorkerFixture>
    {
        private const string TenantId = "13dd96d361a4455f8b1a1ad9b3f43dce";
        private readonly byte[] _tenantSecret = "705424692fda4b86b8726d64b22cb1bf".ToByteArray();
        private readonly IGenericHostTest _host;
        private readonly string _correlationId;
        private readonly CancellationToken _cancellationToken;

        public QueueWorkerTest(WorkerFixture workerFixture, ITestOutputHelper output) : base(output)
        {
            if (workerFixture.ConfigureServicesCallback == null)
            {
                workerFixture.ConfigureServicesCallback = (services) => services.AddXunitTestLogging(TestOutput);
            }
            _correlationId = workerFixture.CorrelationId;
            _host = workerFixture.HostTest;
            _cancellationToken = workerFixture.CancellationToken;
        }

        [Fact, Priority(0)]
        public async Task ExecuteAsync_ShouldCreateAcceptedStatus()
        {
            var queue = _host.ServiceProvider.GetRequiredService<IPointToPointChannel<ICommand, StatusCommandHandler>>();
            var bus = _host.ServiceProvider.GetRequiredService<IPublishSubscribeChannel<IIntegrationEvent, StatusEventHandler>>();
            var marshaller = _host.ServiceProvider.GetRequiredService<IMarshaller>();
            var mediator = _host.ServiceProvider.GetRequiredService<IMediator>();

            var command = new CreateStatusAcceptedCommand(TenantId, _correlationId, OperationScope.Create.ToString(), "https://some.microservice.io/", "Creating a new monitor.");
            var message = command.ToMessage($"urn:status:id:{command.CorrelationId}".ToUri(), "status.accepted").Sign(marshaller, o => o.SignatureSecret = _tenantSecret);
            await queue.SendAsync(message.Yield(), o => o.CancellationToken = _cancellationToken);

            TestOutput.WriteLine(JsonFormatter.SerializeObject(message).ToEncodedString());
            
            var result = await Awaiter.RunUntilSucceededOrTimeoutAsync(async () =>
            {
                var signaled = false;
                await bus.SubscribeAsync((@event, _) =>
                {
                    if (@event.Data is StatusAcceptedEvent created)
                    {
                        Assert.Equal(_correlationId, created.CorrelationId);
                        Assert.Equal(command.Message, created.Message);
                        Assert.Equal(command.Endpoint, created.Endpoint);
                        Assert.Equal(command.Scope, created.Scope);
                        Assert.Equal(Status.Accepted, created.Status);
                        Assert.InRange(created.AcceptedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
                        signaled = true;
                    }
                    return Task.CompletedTask;
                }, o => o.CancellationToken = _cancellationToken);
                return signaled ? new SuccessfulValue() : new UnsuccessfulValue();
            }, o => o.CancellationToken = _cancellationToken);

            Assert.True(result.Succeeded, "Expected to be successful.");

            var status = await mediator.QueryAsync(new GetStatusQuery(TenantId, _correlationId), o => o.CancellationToken = _cancellationToken);

            Assert.Equal(_correlationId, status.CorrelationId);
            Assert.Equal(command.Message, status.Message);
            Assert.Equal(command.Endpoint, status.Endpoint);
            Assert.Equal(command.Scope, status.Scope);
            Assert.Equal(Status.Accepted, status.Status);
            Assert.InRange(status.AcceptedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
            Assert.Null(status.RunningAt);
            Assert.Null(status.FailedAt);
            Assert.Null(status.SucceededAt);
        }

        [Fact, Priority(1)]
        public async Task ExecuteAsync_ShouldUpdateStatusToRunning()
        {
            var queue = _host.ServiceProvider.GetRequiredService<IPointToPointChannel<ICommand, StatusCommandHandler>>();
            var bus = _host.ServiceProvider.GetRequiredService<IPublishSubscribeChannel<IIntegrationEvent, StatusEventHandler>>();
            var marshaller = _host.ServiceProvider.GetRequiredService<IMarshaller>();
            var mediator = _host.ServiceProvider.GetRequiredService<IMediator>();

            var command = new UpdateStatusToRunningCommand(TenantId, _correlationId, "Running like the wind ...");
            var message = command.ToMessage($"urn:status:id:{command.CorrelationId}".ToUri(), "status.running").Sign(marshaller, o => o.SignatureSecret = _tenantSecret);
            await queue.SendAsync(message.Yield(), o => o.CancellationToken = _cancellationToken);

            TestOutput.WriteLine(JsonFormatter.SerializeObject(message).ToEncodedString());

            var result = await Awaiter.RunUntilSucceededOrTimeoutAsync(async () =>
            {
                var signaled = false;
                await bus.SubscribeAsync((@event, _) =>
                {
                    if (@event.Data is StatusRunningEvent running)
                    {
                        Assert.Equal(_correlationId, running.CorrelationId);
                        Assert.Equal(command.Message, running.Message);
                        Assert.Equal(OperationStatus.Running.ToString(), running.Status);
                        Assert.InRange(running.RunningAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
                        signaled = true;
                    }
                    return Task.CompletedTask;
                }, o => o.CancellationToken = _cancellationToken);
                return signaled ? new SuccessfulValue() : new UnsuccessfulValue();
            }, o => o.CancellationToken = _cancellationToken);

            Assert.True(result.Succeeded, "Expected to be successful.");

            var status = await mediator.QueryAsync(new GetStatusQuery(TenantId, _correlationId), o => o.CancellationToken = _cancellationToken);

            Assert.Equal(_correlationId, status.CorrelationId);
            Assert.Equal(command.Message, status.Message);
            Assert.Equal("https://some.microservice.io/", status.Endpoint);
            Assert.Equal(OperationStatus.Running.ToString(), status.Status);
            Assert.Equal(OperationScope.Create.ToString(), status.Scope);
            Assert.InRange(status.AcceptedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
            Assert.InRange(status.RunningAt!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        }

        [Fact, Priority(2)]
        public async Task ExecuteAsync_ShouldUpdateStatusToSucceeded()
        {
            var queue = _host.ServiceProvider.GetRequiredService<IPointToPointChannel<ICommand, StatusCommandHandler>>();
            var bus = _host.ServiceProvider.GetRequiredService<IPublishSubscribeChannel<IIntegrationEvent, StatusEventHandler>>();
            var marshaller = _host.ServiceProvider.GetRequiredService<IMarshaller>();
            var mediator = _host.ServiceProvider.GetRequiredService<IMediator>();

            var command = new UpdateStatusToSucceededCommand(TenantId, _correlationId, "f92f29f3528a456b9c50ba4f05bb7b30");
            var message = command.ToMessage($"urn:status:id:{command.CorrelationId}".ToUri(), "status.succeeded").Sign(marshaller, o => o.SignatureSecret = _tenantSecret);
            await queue.SendAsync(message.Yield(), o => o.CancellationToken = _cancellationToken);

            TestOutput.WriteLine(JsonFormatter.SerializeObject(message).ToEncodedString());

            var result = await Awaiter.RunUntilSucceededOrTimeoutAsync(async () =>
            {
                var signaled = false;
                await bus.SubscribeAsync((@event, _) =>
                {
                    if (@event.Data is StatusSucceededEvent succeeded)
                    {
                        Assert.Equal(_correlationId, succeeded.CorrelationId);
                        Assert.Equal(command.EndpointRouteValue, succeeded.EndpointRouteValue);
                        Assert.Equal(OperationStatus.Succeeded.ToString(), succeeded.Status);
                        Assert.InRange(succeeded.SucceededAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
                        signaled = true;
                    }
                    return Task.CompletedTask;
                }, o => o.CancellationToken = _cancellationToken);
                return signaled ? new SuccessfulValue() : new UnsuccessfulValue();
            }, o => o.CancellationToken = _cancellationToken);

            Assert.True(result.Succeeded, "Expected to be successful.");

            var status = await mediator.QueryAsync(new GetStatusQuery(TenantId, _correlationId), o => o.CancellationToken = _cancellationToken);

            Assert.Equal(_correlationId, status.CorrelationId);
            Assert.Equal("Running like the wind ...", status.Message);
            Assert.Equal("https://some.microservice.io/", status.Endpoint);
            Assert.Equal(command.EndpointRouteValue, status.EndpointRouteValue);
            Assert.Equal(OperationStatus.Succeeded.ToString(), status.Status);
            Assert.Equal(OperationScope.Create.ToString(), status.Scope);
            Assert.InRange(status.AcceptedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
            Assert.InRange(status.RunningAt!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
            Assert.InRange(status.SucceededAt!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        }

        [Fact, Priority(3)]
        public async Task ExecuteAsync_ShouldUpdateStatusToFailed()
        {
            var queue = _host.ServiceProvider.GetRequiredService<IPointToPointChannel<ICommand, StatusCommandHandler>>();
            var bus = _host.ServiceProvider.GetRequiredService<IPublishSubscribeChannel<IIntegrationEvent, StatusEventHandler>>();
            var marshaller = _host.ServiceProvider.GetRequiredService<IMarshaller>();
            var mediator = _host.ServiceProvider.GetRequiredService<IMediator>();

            var command = new UpdateStatusToFailedCommand(TenantId, _correlationId, new NullReferenceException());
            var message = command.ToMessage($"urn:status:id:{command.CorrelationId}".ToUri(), "status.failed").Sign(marshaller, o => o.SignatureSecret = _tenantSecret);
            await queue.SendAsync(message.Yield(), o => o.CancellationToken = _cancellationToken);

            TestOutput.WriteLine(JsonFormatter.SerializeObject(message).ToEncodedString());

            var result = await Awaiter.RunUntilSucceededOrTimeoutAsync(async () =>
            {
                var signaled = false;
                await bus.SubscribeAsync((@event, _) =>
                {
                    if (@event.Data is StatusFailedEvent failed)
                    {
                        Assert.Equal(_correlationId, failed.CorrelationId);
                        Assert.Equal(command.FailedReason, failed.FailedReason);
                        Assert.Equal(OperationStatus.Failed.ToString(), failed.Status);
                        Assert.InRange(failed.FailedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
                        signaled = true;
                    }
                    return Task.CompletedTask;
                }, o => o.CancellationToken = _cancellationToken);
                return signaled ? new SuccessfulValue() : new UnsuccessfulValue();
            }, o => o.CancellationToken = _cancellationToken);

            Assert.True(result.Succeeded, "Expected to be successful.");

            var status = await mediator.QueryAsync(new GetStatusQuery(TenantId, _correlationId), o => o.CancellationToken = _cancellationToken);

            Assert.Equal(_correlationId, status.CorrelationId);
            Assert.Equal("Running like the wind ...", status.Message);
            Assert.Equal("https://some.microservice.io/", status.Endpoint);
            Assert.Equal("f92f29f3528a456b9c50ba4f05bb7b30", status.EndpointRouteValue);
            Assert.Equal(command.FailedReason, status.FailedReason);
            Assert.Equal(OperationStatus.Failed.ToString(), status.Status);
            Assert.Equal(OperationScope.Create.ToString(), status.Scope);
            Assert.InRange(status.AcceptedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
            Assert.InRange(status.RunningAt!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
            Assert.InRange(status.SucceededAt!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
            Assert.InRange(status.FailedAt!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        }

        
        [Fact, Priority(4)]
        public async Task ExecuteAsync_ShouldDeleteStatus()
        {
            var queue = _host.ServiceProvider.GetRequiredService<IPointToPointChannel<ICommand, StatusCommandHandler>>();
            var bus = _host.ServiceProvider.GetRequiredService<IPublishSubscribeChannel<IIntegrationEvent, StatusEventHandler>>();
            var marshaller = _host.ServiceProvider.GetRequiredService<IMarshaller>();
            var mediator = _host.ServiceProvider.GetRequiredService<IMediator>();

            var command = new DeleteStatusCommand(TenantId, _correlationId);
            var message = command.ToMessage($"urn:status:id:{command.CorrelationId}".ToUri(), "status.delete").Sign(marshaller, o => o.SignatureSecret = _tenantSecret);
            await queue.SendAsync(message.Yield(), o => o.CancellationToken = _cancellationToken);

            TestOutput.WriteLine(JsonFormatter.SerializeObject(message).ToEncodedString());

            var result = await Awaiter.RunUntilSucceededOrTimeoutAsync(async () =>
            {
                var signaled = false;
                await bus.SubscribeAsync((@event, _) =>
                {
                    if (@event.Data is StatusDeletedEvent deleted)
                    {
                        Assert.Equal(_correlationId, deleted.CorrelationId);
                        signaled = true;
                    }
                    return Task.CompletedTask;
                }, o => o.CancellationToken = _cancellationToken);
                return signaled ? new SuccessfulValue() : new UnsuccessfulValue();
            }, o => o.CancellationToken = _cancellationToken);

            Assert.True(result.Succeeded, "Expected to be successful.");

            var status = await mediator.QueryAsync(new GetStatusQuery(TenantId, _correlationId), o => o.CancellationToken = _cancellationToken);

            Assert.Null(status);
        }

        protected override void OnDisposeManagedResources()
        {
            _host?.Dispose();
        }
    }
}
