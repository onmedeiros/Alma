namespace Alma.Workflows.Core.States.Machine
{
    /// <summary>
    /// Concrete implementation of the flow state machine.
    /// </summary>
    public class FlowStateMachine : IFlowStateMachine
    {
        private readonly StateTransitionValidator _validator;
        private readonly List<FlowStateTransition> _transitionHistory;
        private FlowState _currentState;

        public FlowStateMachine()
        {
            _validator = new StateTransitionValidator();
            _transitionHistory = new List<FlowStateTransition>();
            _currentState = FlowState.NotStarted;
        }

        public FlowState CurrentState => _currentState;

        public IReadOnlyList<FlowStateTransition> TransitionHistory => _transitionHistory.AsReadOnly();

        public bool TransitionTo(FlowState newState, string? reason = null)
        {
            if (!_validator.IsTransitionAllowed(_currentState, newState))
                return false;

            var transition = new FlowStateTransition(_currentState, newState, reason);
            _transitionHistory.Add(transition);
            _currentState = newState;

            return true;
        }

        public bool CanTransitionTo(FlowState newState)
        {
            return _validator.IsTransitionAllowed(_currentState, newState);
        }

        public IEnumerable<FlowState> GetAllowedTransitions()
        {
            return _validator.GetAllowedTransitions(_currentState);
        }

        public bool IsInTerminalState()
        {
            return _validator.IsTerminalState(_currentState);
        }

        public void Reset()
        {
            _currentState = FlowState.NotStarted;
            _transitionHistory.Clear();
        }
    }
}
