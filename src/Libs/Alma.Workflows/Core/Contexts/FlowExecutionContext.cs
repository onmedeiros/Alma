using Alma.Workflows.Options;
using Alma.Workflows.States;

namespace Alma.Workflows.Core.Contexts
{
    public class FlowExecutionContext : ActivityExecutionContext
    {
        public Flow Flow { get; init; }

        public FlowExecutionContext(Flow flow, IServiceProvider serviceProvider)
            : this(flow, serviceProvider, new ExecutionState())
        {
        }

        public FlowExecutionContext(Flow flow, IServiceProvider serviceProvider, ExecutionState state)
            : this(flow, serviceProvider, state, new ExecutionOptions())
        {
        }

        public FlowExecutionContext(Flow flow, IServiceProvider serviceProvider, ExecutionState state, ExecutionOptions options) : base(serviceProvider, state, options)
        {
            Flow = flow;
        }
    }
}