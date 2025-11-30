using Alma.Workflows.Options;
using Alma.Workflows.States;

namespace Alma.Workflows.Core.Contexts
{
    public class WorkflowExecutionContext : ActivityExecutionContext
    {
        public Workflow Flow { get; init; }

        public WorkflowExecutionContext(Workflow flow, IServiceProvider serviceProvider)
            : this(flow, serviceProvider, new ExecutionOptions())
        {
        }

        public WorkflowExecutionContext(Workflow flow, IServiceProvider serviceProvider, ExecutionOptions options) : base(serviceProvider, options)
        {
            Flow = flow;
        }
    }
}