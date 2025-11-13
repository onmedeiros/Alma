namespace Alma.Workflows.Core.States.Machine
{
    /// <summary>
    /// Validates state transitions to ensure they follow allowed rules.
    /// </summary>
    public class StateTransitionValidator
    {
        private static readonly Dictionary<FlowState, HashSet<FlowState>> _allowedTransitions = new()
        {
            [FlowState.NotStarted] = new HashSet<FlowState> 
            { 
                FlowState.Running, 
                FlowState.Cancelled 
            },
            [FlowState.Running] = new HashSet<FlowState> 
            { 
                FlowState.Waiting, 
                FlowState.Paused, 
                FlowState.Completed, 
                FlowState.Failed, 
                FlowState.Cancelled 
            },
            [FlowState.Waiting] = new HashSet<FlowState> 
            { 
                FlowState.Running, 
                FlowState.Cancelled 
            },
            [FlowState.Paused] = new HashSet<FlowState> 
            { 
                FlowState.Running, 
                FlowState.Cancelled 
            },
            [FlowState.Completed] = new HashSet<FlowState>(),
            [FlowState.Failed] = new HashSet<FlowState>(),
            [FlowState.Cancelled] = new HashSet<FlowState>()
        };

        /// <summary>
        /// Validates if a transition from one state to another is allowed.
        /// </summary>
        public bool IsTransitionAllowed(FlowState from, FlowState to)
        {
            if (from == to)
                return true; // Allow self-transitions

            return _allowedTransitions.TryGetValue(from, out var allowedStates) 
                && allowedStates.Contains(to);
        }

        /// <summary>
        /// Gets all allowed transitions from a given state.
        /// </summary>
        public IEnumerable<FlowState> GetAllowedTransitions(FlowState from)
        {
            return _allowedTransitions.TryGetValue(from, out var allowedStates) 
                ? allowedStates 
                : Enumerable.Empty<FlowState>();
        }

        /// <summary>
        /// Checks if a state is terminal (no transitions allowed from it).
        /// </summary>
        public bool IsTerminalState(FlowState state)
        {
            return _allowedTransitions.TryGetValue(state, out var allowedStates) 
                && allowedStates.Count == 0;
        }
    }
}
