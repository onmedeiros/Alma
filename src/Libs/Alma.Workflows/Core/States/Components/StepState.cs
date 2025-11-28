using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.States.Abstractions;

namespace Alma.Workflows.Core.States.Components
{
    public class StepState : StateComponent, IStepState
    {
        private const string STATE_KEY = "Steps";

        public ActivityStepData? GetStepData(string activityId, string stepId)
        {
            var state = GetState();

            if (state.TryGetValue(activityId, out var steps))
            {
                return steps.FirstOrDefault(s => s.Id == stepId);
            }

            return null;
        }

        public void SetStepData(ActivityStepData stepData)
        {
            var state = GetState();

            if (!state.TryGetValue(stepData.ActivityId, out var steps))
            {
                steps = new List<ActivityStepData>();
                state[stepData.ActivityId] = steps;
            }

            var existingStep = steps.FirstOrDefault(s => s.Id == stepData.Id);

            if (existingStep != null)
            {
                steps.Remove(existingStep);
            }

            steps.Add(stepData);
        }

        private Dictionary<string, List<ActivityStepData>> GetState()
        {
            EnsureInitialized();
            return StateData!.Steps;
        }
    }
}