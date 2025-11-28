using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Models.Activities;
using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Data
{
    public class StateData
    {
        public Dictionary<string, ValueObject> Parameters { get; set; } = [];

        public Dictionary<string, ValueObject> Variables { get; set; } = [];

        public List<MemoryData> Memory { get; set; } = [];

        public List<QueueItem> Queue { get; set; } = [];

        public Dictionary<string, List<ActivityStepData>> Steps { get; set; } = [];

        public List<ApprovalStateData> Approvals { get; set; } = [];

        public List<ExecutedConnection> Connections { get; set; } = [];

        public List<LogModel> Logs { get; set; } = [];
    }
}