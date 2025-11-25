using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;

namespace Alma.Workflows
{
    public class Flow : Activity
    {
        public IActivity? Start { get; set; }
        public ICollection<IActivity> Activities { get; set; } = [];
        public ICollection<Connection> Connections { get; set; } = [];

        public async ValueTask ExecuteAsync(WorkflowExecutionContext context)
        {
        }

        public IActivity GetStart()
        {
            var startActivity = Activities.FirstOrDefault(x => x is IStart)
                ?? throw new InvalidOperationException("Start activity not defined.");

            return startActivity;
        }
    }
}