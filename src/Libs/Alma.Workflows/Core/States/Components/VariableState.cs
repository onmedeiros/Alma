using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.States.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace Alma.Workflows.Core.States.Components
{
    public class VariableState : StateComponent, IVariableState
    {
        public void Set(string name, object? value)
        {
            var state = GetState();

            state.Remove(name);

            state.Add(name, new ValueObject(value));
        }

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
            throw new NotImplementedException();
        }

        public T? GetValue<T>(string name)
        {
            throw new NotImplementedException();
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

        public bool Contains(string name)
        {
            return GetState().ContainsKey(name);
        }

        private Dictionary<string, ValueObject> GetState()
        {
            EnsureInitialized();

            if (StateData!.TryGetValue("Variables", out var stateObj) && stateObj is Dictionary<string, ValueObject> state)
            {
                return state;
            }

            var newState = new Dictionary<string, ValueObject>();

            StateData["Variables"] = newState;

            return newState;
        }
    }
}