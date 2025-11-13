namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Concrete implementation of parameter state management.
    /// </summary>
    public class ParameterState : IParameterState
    {
        private readonly Dictionary<string, object?> _parameters;

        public ParameterState()
        {
            _parameters = new Dictionary<string, object?>();
        }

        public int Count => _parameters.Count;

        public IReadOnlyDictionary<string, object?> GetAll()
        {
            return _parameters;
        }

        public object? Get(string name)
        {
            return _parameters.TryGetValue(name, out var value) ? value : null;
        }

        public void Set(string name, object? value)
        {
            _parameters[name] = value;
        }

        public bool Contains(string name)
        {
            return _parameters.ContainsKey(name);
        }

        public bool Remove(string name)
        {
            return _parameters.Remove(name);
        }

        public void Clear()
        {
            _parameters.Clear();
        }

        public Dictionary<string, object?> GetTemplateParameters()
        {
            return new Dictionary<string, object?>(_parameters);
        }
    }
}
