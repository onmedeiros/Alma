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

        private Dictionary<string, ICollection<ActivityStepData>> GetState()
        {
            EnsureInitialized();

            if (StateData!.TryGetValue(STATE_KEY, out var stateObj) && stateObj is Dictionary<string, ICollection<ActivityStepData>> state)
            {
                return state;
            }

            var newState = new Dictionary<string, ICollection<ActivityStepData>>();

            StateData[STATE_KEY] = newState;
            return newState;
        }
    }
}