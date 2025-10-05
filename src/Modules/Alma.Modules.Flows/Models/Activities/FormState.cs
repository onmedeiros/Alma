namespace Alma.Modules.Flows.Models.Activities
{
    public class FormState
    {
        private Dictionary<string, object?> _state = [];

        public event Action? OnStateChanged;

        public T? GetValue<T>(string name)
        {
            if (!_state.ContainsKey(name))
                _state[name] = default(T);

            if (_state[name] is T value)
                return value;
            else
                return default;
        }

        public void SetValue<T>(string name, T value)
        {
            _state.Remove(name);
            _state.Add(name, value);

            OnStateChanged?.Invoke();
        }

        public void SetState(Dictionary<string, object?> state)
        {
            _state = state;
        }

        public Dictionary<string, object?> GetState()
        {
            return _state;
        }
    }
}
