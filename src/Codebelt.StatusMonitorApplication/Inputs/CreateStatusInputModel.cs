using Codebelt.StatusMonitor;

namespace Codebelt.StatusMonitorApplication.Inputs
{
    public class CreateStatusInputModel
    {
        public string CorrelationId { get; set; }

        public string Message { get; set; }

        public string Endpoint { get; set; }

        public OperationScope Scope { get; set; }
    }
}
