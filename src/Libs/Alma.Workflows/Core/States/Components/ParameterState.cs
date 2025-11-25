using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.States.Abstractions;

namespace Alma.Workflows.Core.States.Components
{
    public class ParameterState : StateComponent, IParameterState
    {
        public Dictionary<string, ValueObject> AsDictionary()
        {
            return GetState();
        }

        public Dictionary<string, object?> AsObjectDictionary()
        {
            return GetState().ToDictionary(kv => kv.Key, kv => kv.Value.GetValue());
        }

        public ValueObject? Get(string name)
        {
            return GetState().TryGetValue(name, out var value) ? value : null;
        }

        public T? GetValue<T>(string name)
        {
            if (!TryGet(name, out var objValue))
            {
                return default;
            }
            var desserialized = objValue!.GetDesserializedValue();
            if (desserialized is T typedValue)
            {
                return typedValue;
            }
            throw new InvalidOperationException("The value cannot be cast to the specified type.");
        }

        public void Set(string name, object? value)
        {
            GetState()[name] = new ValueObject(value);
        }

        public bool TryGet(string name, out ValueObject? value)
        {
            return GetState().TryGetValue(name, out value);
        }

        public bool TryGetValue<T>(string name, out T? value)
        {
            if (!TryGet(name, out var objValue))
            {
                value = default;
                return false;
            }

            var desserialized = objValue!.GetDesserializedValue();

            if (desserialized is T typedValue)
            {
                value = typedValue;
                return true;
            }

            throw new InvalidOperationException("The value cannot be cast to the specified type.");
        }

        private Dictionary<string, ValueObject> GetState()
        {
            EnsureInitialized();

            if (StateData!.TryGetValue("Parameters", out var stateObj) && stateObj is Dictionary<string, ValueObject> state)
            {
                return state;
            }

            var newState = new Dictionary<string, ValueObject>();

            StateData["Parameters"] = newState;

            return newState;
        }
    }
}