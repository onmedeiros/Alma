using Alma.Workflows.Core.States.Abstractions;
using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Components
{
    public class ConnectionState : StateComponent, IConnectionState
    {
        private const string STATE_KEY = "Connections";

        public ICollection<ExecutedConnection> AsCollection()
        {
            return GetState();
        }

        public void Add(ExecutedConnection executedConnection)
        {
            var state = GetState();
            state.Add(executedConnection);
        }

        private ICollection<ExecutedConnection> GetState()
        {
            EnsureInitialized();

            if (StateData!.TryGetValue(STATE_KEY, out var stateObj) && stateObj is ICollection<ExecutedConnection> state)
            {
                return state;
            }

            var newState = new List<ExecutedConnection>();

            StateData[STATE_KEY] = newState;

            return newState;
        }
    }
}