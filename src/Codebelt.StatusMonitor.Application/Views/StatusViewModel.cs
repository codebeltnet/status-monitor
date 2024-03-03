using System;

namespace Codebelt.StatusMonitor.Application.Views
{
    public class StatusViewModel
    {
        public StatusViewModel(string correlationId, string message, string endpoint, string scope, string status)
        {
            CorrelationId = correlationId;
            Message = message;
            Endpoint = endpoint;
            Scope = scope;
            Status = status;
        }

        public string CorrelationId { get; set; }

        public string Message { get; set; }

        public string Endpoint { get; set; }

        public string? EndpointRouteValue { get; set; }

        public string Scope { get; set; }

        public string Status { get; set; }

        public string? FailedReason { get; set; }

        public DateTime AcceptedAt { get; set; }

        public DateTime? RunningAt { get; set; }

        public DateTime? SucceededAt { get; set; }

        public DateTime? FailedAt { get; set; }
    }
}
