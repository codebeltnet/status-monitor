using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Codebelt.Bootstrapper;
using Codebelt.Shared;
using Codebelt.StatusMonitor.Application;
using Codebelt.StatusMonitor.Application.Commands;
using Cuemon.Extensions;
using Cuemon.Extensions.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Savvyio;
using Savvyio.Commands;
using Savvyio.Extensions;
using Savvyio.Extensions.DependencyInjection.Messaging;
using Savvyio.Messaging.Cryptography;

namespace Codebelt.StatusMonitor.Worker
{
    public class QueueWorker : BackgroundService
    {
        private bool _applicationStopping = false;
        private readonly ILogger<QueueWorker> _logger;
        private readonly IHostEnvironment _environment;
        private readonly IMarshaller _marshaller;
        private readonly IMediator _mediator;
        private readonly ITenantDataStore _tenantDataStore;
        private readonly IPointToPointChannel<ICommand, StatusCommandHandler> _queue;

        public QueueWorker(ILogger<QueueWorker> logger,
            IServiceProvider serviceProvider,
            IHostEnvironment environment,
            IMarshaller marshaller,
            IMediator mediator,
            ITenantDataStore tenantDataStore,
            IPointToPointChannel<ICommand, StatusCommandHandler> queue)
        {
            BootstrapperLifetime.OnApplicationStartedCallback = serviceProvider.UseHandlerServicesDescriptor;
            BootstrapperLifetime.OnApplicationStoppingCallback = () =>
            {
                _applicationStopping = true;
            };
            _logger = logger;
            _environment = environment;
            _marshaller = marshaller;
            _mediator = mediator;
            _tenantDataStore = tenantDataStore;
            _queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_applicationStopping) { return; }

                try
                {
                    await foreach (var message in _queue.ReceiveAsync(o => o.CancellationToken = stoppingToken).ConfigureAwait(false))
                    {
                        _logger.LogInformation("Processing: {message}", message);

                        if (message is ISignedMessage<ICommand> signedMessage && signedMessage.Data is TenantCommand command) // only process cryptographically signed messages with embedded tenant command
                        {
                            var tenant = (await _tenantDataStore.FindAllAsync(o => o.Filter = tenant => tenant.Id == command.TenantId).ConfigureAwait(false)).SingleOrDefault();
                            if (tenant == null)
                            {
                                _logger.LogWarning("Invalid tenant: '{tenantId}'.", command.TenantId);
                                return;
                            }

                            var secrets = tenant.AccessKeys.Where(key => key.Enabled && key.Expires > DateTime.UtcNow).Select(key => key.Secret).ToList();
                            foreach (var secret in secrets)
                            {
                                try
                                {
                                    signedMessage.CheckSignature(_marshaller, o => o.SignatureSecret = secret.ToByteArray());
                                    await _mediator.CommitAsync(signedMessage.Data).ConfigureAwait(false);
                                }
                                catch (ArgumentOutOfRangeException ex)
                                {
                                    _logger.LogWarning(ex, "Tenant '{tenantId}' has invalid signature for message: {signedMessage}", command.TenantId, _marshaller.Serialize(signedMessage).ToEncodedString());
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }

                if (_environment.IsProduction())
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }
    }
}
