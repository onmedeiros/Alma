using Alma.Workflows.Core.Activities.Enums;
using Alma.Workflows.Core.Contexts;

namespace Alma.Workflows.Core.Activities.Abstractions
{
    public interface IStep
    {
        string Id { get; }

        IActivity Activity { get; }

        void SetId(string Id);

        void SetActivity(IActivity activity);

        ValueTask<ActivityStepStatus> GetStatus(ActivityExecutionContext context);

        ValueTask<ActivityStepStatus> ExecuteAsync(ActivityExecutionContext context);
    }
}