using Alma.Workflows.Models;

using Alma.Workflows.Core.ApprovalsAndChecks.Models;

namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Concrete implementation of variable state management.
    /// </summary>
    public class VariableState : IVariableState
    {
        private readonly Dictionary<string, ValueObject> _variables;

        public VariableState()
        {
            _variables = new Dictionary<string, ValueObject>();
        }

        public int Count => _variables.Count;

        public IReadOnlyDictionary<string, ValueObject> GetAll()
        {
            return _variables;
        }

        public ValueObject? Get(string name)
        {
            return _variables.TryGetValue(name, out var value) ? value : null;
        }

        public void Set(string name, object? value)
        {
            _variables.Remove(name);
            _variables.Add(name, new ValueObject(value));
        }

        public bool Contains(string name)
        {
            return _variables.ContainsKey(name);
        }

        public bool Remove(string name)
        {
            return _variables.Remove(name);
        }

        public void Clear()
        {
            _variables.Clear();
        }
    }
}
