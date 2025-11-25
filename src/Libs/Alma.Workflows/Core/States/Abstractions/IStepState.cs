using Alma.Workflows.Core.Activities.Models;

namespace Alma.Workflows.Core.States.Abstractions
{
    public interface IStepState : IStateComponent
    {
        ActivityStepData? GetStepData(string activityId, string stepId);

        void SetStepData(ActivityStepData stepData);
    }
}