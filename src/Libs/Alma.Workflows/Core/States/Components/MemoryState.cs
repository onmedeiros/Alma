using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.States.Abstractions;
using Alma.Workflows.Core.States.Data;

namespace Alma.Workflows.Core.States.Components
{
    public class MemoryState : StateComponent, IMemoryState
    {
        public void Set(string activityId, string key, object? value)
        {
            var memoryData = EnsureActivityMemoryData(activityId);
            memoryData.Data[key] = new ValueObject(value);
        }

        public ValueObject Get(string activityId, string key)
        {
            var memoryData = EnsureActivityMemoryData(activityId);

            if (!memoryData.Data.TryGetValue(key, out var value))
            {
                value = new ValueObject(null);
                memoryData.Data[key] = value;
            }

            return value;
        }

        public T? Get<T>(string activityId, string key)
        {
            var valueObject = Get(activityId, key);
            if (valueObject.GetValue() is T typedValue)
            {
                return typedValue;
            }
            else
            {
                return default(T);
            }
        }

        private List<MemoryData> GetState()
        {
            EnsureInitialized();

            const string STATE_KEY = "Memory";

            if (StateData!.TryGetValue(STATE_KEY, out var stateObj) && stateObj is List<MemoryData> state)
            {
                return state;
            }

            var newState = new List<MemoryData>();

            StateData[STATE_KEY] = newState;

            return newState;
        }

        private MemoryData EnsureActivityMemoryData(string activityId)
        {
            var state = GetState();

            if (!state.Any(md => md.ActivityId == activityId))
            {
                var memoryData = new MemoryData { ActivityId = activityId };
                state.Add(memoryData);
                return memoryData;
            }

            return state.First(md => md.ActivityId == activityId);
        }
    }
}