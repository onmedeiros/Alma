namespace Alma.Workflows.Core.States.Machine
{
    /// <summary>
    /// Interface for flow state machine.
    /// </summary>
    public interface IFlowStateMachine
    {
        /// <summary>
        /// Gets the current state.
        /// </summary>
        FlowState CurrentState { get; }

        /// <summary>
        /// Gets all state transitions history.
        /// </summary>
        IReadOnlyList<FlowStateTransition> TransitionHistory { get; }

        /// <summary>
        /// Transitions to a new state.
        /// </summary>
        /// <param name="newState">The target state.</param>
        /// <param name="reason">Optional reason for the transition.</param>
        /// <returns>True if transition was successful, false if not allowed.</returns>
        bool TransitionTo(FlowState newState, string? reason = null);

        /// <summary>
        /// Checks if a transition to the specified state is allowed.
        /// </summary>
        bool CanTransitionTo(FlowState newState);

        /// <summary>
        /// Gets all allowed transitions from the current state.
        /// </summary>
        IEnumerable<FlowState> GetAllowedTransitions();

        /// <summary>
        /// Checks if the current state is terminal.
        /// </summary>
        bool IsInTerminalState();

        /// <summary>
        /// Resets the state machine to NotStarted.
        /// </summary>
        void Reset();
    }
}
