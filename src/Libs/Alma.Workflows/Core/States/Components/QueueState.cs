using Alma.Workflows.Core.States.Abstractions;
using Alma.Workflows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Core.States.Components
{
    public class QueueState : StateComponent, IQueueState
    {
        private const string STATE_KEY = "Queue";

        private readonly ILogger<QueueState> _logger;

        public QueueState(ILogger<QueueState> logger)
        {
            _logger = logger;
        }

        public IReadOnlyCollection<QueueItem> AsCollection()
        {
            return GetState();
        }

        public void Add(QueueItem item)
        {
            var state = GetState();
            state.Add(item);
        }

        private List<QueueItem> GetState()
        {
            EnsureInitialized();

            if (StateData!.TryGetValue(STATE_KEY, out var stateObj) && stateObj is List<QueueItem> state)
            {
                return state;
            }

            var newState = new List<QueueItem>();

            StateData[STATE_KEY] = newState;

            return newState;
        }
    }
}