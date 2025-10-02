using Alma.Flows.Options;
using Alma.Flows.States;

namespace Alma.Flows.Core.Contexts
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