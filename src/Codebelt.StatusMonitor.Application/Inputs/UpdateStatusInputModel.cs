using System;

namespace Codebelt.StatusMonitor.Application.Inputs
{
    public class UpdateStatusInputModel
    {
        public string? Message { get; set; }

        public string? Endpoint { get; set; }

        public string? FailedReason { get; set; }

        public string? EndpointRouteValue { get; set; }

        public DateTime? FailedAt { get; set; }

        public DateTime? RunningAt { get; set; }

        public DateTime? SucceededAt { get; set; }
    }
}
