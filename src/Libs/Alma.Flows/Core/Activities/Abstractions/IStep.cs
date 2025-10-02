using Alma.Flows.Core.Abstractions;
using Alma.Flows.Core.Activities.Enums;
using Alma.Flows.Core.Contexts;

namespace Alma.Flows.Core.Activities.Abstractions
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