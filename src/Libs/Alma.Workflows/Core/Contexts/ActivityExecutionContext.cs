using Alma.Workflows.Core.States.Abstractions;
using Alma.Workflows.Options;
using Alma.Workflows.States;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows.Core.Contexts
{
    public class ActivityExecutionContext
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public IServiceProvider ServiceProvider { get; set; }
        public IExecutionState State { get; set; }
        public ExecutionOptions Options { get; set; }

        public ActivityExecutionContext(IServiceProvider serviceProvider, ExecutionOptions options)
        {
            ServiceProvider = serviceProvider;
            State = ServiceProvider.GetRequiredService<IExecutionState>();
            Options = options;
        }
    }
}