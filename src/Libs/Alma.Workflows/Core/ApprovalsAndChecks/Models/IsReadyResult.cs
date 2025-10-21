namespace Alma.Workflows.Core.ApprovalsAndChecks.Models
{
    public class IsReadyResult
    {
        public bool IsReady { get; set; } = true;

        public string? Reason { get; set; }

        public static IsReadyResult Ready() => new IsReadyResult();

        public static IsReadyResult NotReady() => new IsReadyResult { IsReady = false };

        public static IsReadyResult NotReady(string reason) => new IsReadyResult { IsReady = false, Reason = reason };
    }
}