using Alma.Workflows.Options;
using Alma.Workflows.States;

namespace Alma.Workflows.Core.Contexts
{
    public class ActivityExecutionContext
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public IServiceProvider ServiceProvider { get; set; }
        public ExecutionState State { get; set; }
        public ExecutionOptions Options { get; set; }

        public ActivityExecutionContext(IServiceProvider serviceProvider, ExecutionState state, ExecutionOptions options)
        {
            ServiceProvider = serviceProvider;
            State = state;
            Options = options;
        }
    }
}
