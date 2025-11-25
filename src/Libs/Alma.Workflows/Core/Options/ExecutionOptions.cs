using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.InstanceExecutions.Enums;

namespace Alma.Workflows.Options
{
    public class ExecutionOptions
    {
        public InstanceExecutionMode ExecutionMode { get; set; }

        public bool StepByStep { get; set; }
        public bool MultiTask { get; set; }
        public int MaxDegreeOfParallelism { get; set; } = 1;
        public int Delay { get; set; }
        public Dictionary<string, ValueObject> Parameters { get; set; } = [];
    }
}