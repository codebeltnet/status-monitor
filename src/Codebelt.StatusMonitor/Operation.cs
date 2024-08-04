using Savvyio.Domain;
using System;
using Codebelt.SharedKernel;
using Cuemon;

namespace Codebelt.StatusMonitor
{
    public class Operation : Entity<string>
    {
        Operation()
        {
        }

        public Operation(TenantId tenantId, CorrelationId correlationId) : base(correlationId)
        {
            TenantId = tenantId;
        }

        public Guid TenantId { get; }

        public string Message { get; private set; }

        public string Endpoint { get; private set; }

        public string EndpointRouteValue { get; private set; }

        public string Status { get; private set; }

        public string Scope { get; private set; }

        public string FailedReason { get; private set; }

        public DateTime AcceptedAt { get; private set; }

        public DateTime? RunningAt { get; private set; }

        public DateTime? SucceededAt { get; private set; }

        public DateTime? FailedAt { get; private set; }

        public Operation Accepted(Scope scope, Endpoint endpoint, Message message, CoordinatedUniversalTime acceptedAt)
        {
            Validator.ThrowIfNull(scope);
            Validator.ThrowIfNull(endpoint);
            Validator.ThrowIfNull(message);
            Validator.ThrowIfNull(acceptedAt);
            Scope = scope;
            Status = new Status(StatusMonitor.Status.Accepted);
            Endpoint = endpoint;
            Message = message;
            AcceptedAt = acceptedAt;
            return this;
        }

        public Operation Running(Message message = null, CoordinatedUniversalTime runningAt = null)
        {
            if (message != null) { Message = message; }
            if (runningAt != null) { RunningAt = runningAt; }
            Status = new Status(OperationStatus.Running);
            return this;
        }

        public Operation Succeeded(EndpointRouteValue endpointRouteValue = null, CoordinatedUniversalTime succeededAt = null)
        {
            if (endpointRouteValue != null) { EndpointRouteValue = endpointRouteValue; }
            if (succeededAt != null) { SucceededAt = succeededAt; }
            Status = new Status(OperationStatus.Succeeded);
            return this;
        }

        public Operation Failed(FailedReason failedReason, CoordinatedUniversalTime failedAt = null)
        {
            Validator.ThrowIfNull(failedReason);
            FailedReason = failedReason;
            if (failedAt != null) { FailedAt = failedAt; }
            Status = new Status(OperationStatus.Failed);
            return this;
        }
    }
}
