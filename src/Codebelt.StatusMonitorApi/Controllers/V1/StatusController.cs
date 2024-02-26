using System;
using System.Threading.Tasks;
using Codebelt.StatusMonitor;
using Codebelt.StatusMonitorApplication.Commands;
using Codebelt.StatusMonitorApplication.Inputs;
using Codebelt.StatusMonitorApplication.Queries;
using Codebelt.StatusMonitorApplication.Views;
using Cuemon.AspNetCore.Http;
using Cuemon.AspNetCore.Http.Headers;
using Cuemon.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Savvyio.Commands;
using Savvyio.Extensions;
using Endpoint = Codebelt.StatusMonitor.Endpoint;

namespace Codebelt.StatusMonitorApi.Controllers.V1
{
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
	    private readonly IMediator _mediator;
        private readonly ApiKeySentinelOptions _apiKeyOptions;
        private readonly ILogger<StatusController> _logger;

        public StatusController(IMediator mediator, IOptions<ApiKeySentinelOptions> apiKeyOptions, ILogger<StatusController> logger)
        {
	        _mediator = mediator;
            _apiKeyOptions = apiKeyOptions.Value;
            _logger = logger;
        }

        [HttpPatch("{correlationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StatusViewModel>> Patch([FromRoute] string correlationId, [BindRequired] [FromQuery] OperationStatus status, [FromBody] UpdateStatusInputModel input)
        {
            var tenantQuery = await GetTenantAsync().ConfigureAwait(false);
            Command command = null;
            switch (status)
            {
                case OperationStatus.Running:
                    command = new RunningOperationCommand(
                        tenantQuery.TenantId, 
                        correlationId, 
                        string.IsNullOrWhiteSpace(input.Message) ? null : new Message(input.Message),
                        input.RunningAt.HasValue ? new CoordinatedUniversalTime(input.RunningAt.Value) : null);
                    break;
                case OperationStatus.Succeeded:
                    command = new SucceededOperationCommand(
                        tenantQuery.TenantId, 
                        correlationId, 
                        string.IsNullOrWhiteSpace(input.EndpointRouteValue) ? null : new EndpointRouteValue(input.EndpointRouteValue),
                        input.SucceededAt.HasValue ? new CoordinatedUniversalTime(input.SucceededAt.Value) : null);
                    break;
                case OperationStatus.Failed:
                    command = new FailedOperationCommand(
                        tenantQuery.TenantId,
                        correlationId,
                        new FailedReason(input.FailedReason),
                        input.FailedAt.HasValue ? new CoordinatedUniversalTime(input.FailedAt.Value) : null);
                    break;
            }
            await _mediator.CommitAsync(command).ConfigureAwait(false);
            return Ok(await _mediator.QueryAsync(new GetStatusQuery(tenantQuery.TenantId, correlationId)).ConfigureAwait(false));
        }

        [HttpDelete("{correlationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<StatusViewModel>> Delete([FromRoute] string correlationId)
        {
            var tenantQuery = await GetTenantAsync().ConfigureAwait(false);
            var command = new DeleteOperationCommand(tenantQuery.TenantId, correlationId);
            await _mediator.CommitAsync(command).ConfigureAwait(false);
            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StatusViewModel>> Post([FromBody] CreateStatusInputModel input)
        {
            var tenantQuery = await GetTenantAsync().ConfigureAwait(false);
            var command = new AcceptedOperationCommand(tenantQuery.TenantId, input.CorrelationId, new Scope(input.Scope), new Endpoint(input.Endpoint), new Message(input.Message));
            await _mediator.CommitAsync(command).ConfigureAwait(false);
            return Ok(new StatusViewModel(command.CorrelationId, command.Message, command.Endpoint, command.Scope, command.Status)
            {
                AcceptedAt = command.AcceptedAt
            });
        }

        [HttpGet("{correlationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status303SeeOther)]
        public async Task<ActionResult<StatusViewModel>> Get([FromRoute] string correlationId)
        {
            var tenantQuery = await GetTenantAsync().ConfigureAwait(false);
            var statusQuery = await _mediator.QueryAsync(new GetStatusQuery(tenantQuery.TenantId, correlationId)).ConfigureAwait(false);
            if (statusQuery == null) { return NotFound(); }
            if (statusQuery.Status == Status.Accepted) { return Ok(statusQuery); }
            switch (Enum.Parse<OperationStatus>(statusQuery.Status))
            {
                case OperationStatus.Succeeded:
                    switch (Enum.Parse<OperationScope>(statusQuery.Scope))
                    {
                        case OperationScope.Delete:
                            return NoContent();
                        default:
                            return new SeeOtherResult(new Uri(string.Concat(statusQuery.Endpoint, statusQuery.EndpointRouteValue)));
                    }
                    
                default:
					return Ok(statusQuery);
            }
        }

        private async Task<TenantViewModel> GetTenantAsync()
        {
            return await _mediator.QueryAsync(new GetTenantQuery(HttpContext.Request.Headers[_apiKeyOptions.HeaderName])).ConfigureAwait(false) ?? throw new UnauthorizedException("API key is either invalid, revoked or expired.");
        }
    }
}
