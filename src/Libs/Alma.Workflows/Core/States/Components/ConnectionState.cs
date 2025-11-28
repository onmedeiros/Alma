using Alma.Workflows.Core.States.Abstractions;
using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Components
{
    public class ConnectionState : StateComponent, IConnectionState
    {
        public ICollection<ExecutedConnection> AsCollection()
        {
            return GetState();
        }

        public void Add(ExecutedConnection executedConnection)
        {
            var state = GetState();
            state.Add(executedConnection);
        }

        private List<ExecutedConnection> GetState()
        {
            EnsureInitialized();
            return StateData!.Connections;
        }
    }
}