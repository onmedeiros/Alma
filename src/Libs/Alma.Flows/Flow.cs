using Alma.Flows.Core.Abstractions;
using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Contexts;

namespace Alma.Flows
{
    public class Flow : Activity
    {
        public IActivity? Start { get; set; }
        public ICollection<IActivity> Activities { get; set; } = [];
        public ICollection<Connection> Connections { get; set; } = [];

        public async ValueTask ExecuteAsync(FlowExecutionContext context)
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