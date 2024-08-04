using System;
using System.Threading.Tasks;
using Codebelt.SharedKernel;
using Codebelt.StatusMonitor.Application.Commands;
using Codebelt.StatusMonitor.Application.Inputs;
using Codebelt.StatusMonitor.Application.Queries;
using Codebelt.StatusMonitor.Application.Views;
using Cuemon.AspNetCore.Http;
using Cuemon.AspNetCore.Mvc;
using Cuemon.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Savvyio.Commands;
using Savvyio.Extensions;

namespace Codebelt.StatusMonitor.RestApi.Controllers.V1
{
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
	    private readonly IMediator _mediator;
        private readonly ILogger<StatusController> _logger;

        public StatusController(IMediator mediator, ILogger<StatusController> logger)
        {
	        _mediator = mediator;
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
                    command = new UpdateStatusToRunningCommand(
                        tenantQuery.TenantId, 
                        correlationId, 
                        string.IsNullOrWhiteSpace(input.Message) ? null : new Message(input.Message),
                        input.RunningAt.HasValue ? new CoordinatedUniversalTime(input.RunningAt.Value) : null);
                    break;
                case OperationStatus.Succeeded:
                    command = new UpdateStatusToSucceededCommand(
                        tenantQuery.TenantId, 
                        correlationId, 
                        string.IsNullOrWhiteSpace(input.EndpointRouteValue) ? null : new EndpointRouteValue(input.EndpointRouteValue),
                        input.SucceededAt.HasValue ? new CoordinatedUniversalTime(input.SucceededAt.Value) : null);
                    break;
                case OperationStatus.Failed:
                    command = new UpdateStatusToFailedCommand(
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
            var command = new DeleteStatusCommand(tenantQuery.TenantId, correlationId);
            await _mediator.CommitAsync(command).ConfigureAwait(false);
            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StatusViewModel>> Post([FromBody] CreateStatusInputModel input)
        {
            var tenantQuery = await GetTenantAsync().ConfigureAwait(false);
            var command = new CreateStatusAcceptedCommand(tenantQuery.TenantId, input.CorrelationId, new Scope(input.Scope), new Endpoint(input.Endpoint), new Message(input.Message));
            await _mediator.CommitAsync(command).ConfigureAwait(false);
            return Created((string)null, new StatusViewModel(command.CorrelationId, command.Message, command.Endpoint, command.Scope, command.Status)
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
            return await _mediator.QueryAsync(new GetTenantQuery(HttpContext.Request.Headers[HttpHeaderNames.XApiKey])).ConfigureAwait(false) ?? throw new UnauthorizedException("API key is either invalid, revoked or expired.");
        }
    }
}
