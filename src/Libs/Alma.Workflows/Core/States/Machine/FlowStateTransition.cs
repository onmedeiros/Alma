namespace Alma.Workflows.Core.States.Machine
{
    /// <summary>
    /// Represents a state transition in the flow state machine.
    /// </summary>
    public class FlowStateTransition
    {
        public FlowStateTransition(FlowState from, FlowState to, string? reason = null)
        {
            From = from;
            To = to;
            Reason = reason;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// The state transitioning from.
        /// </summary>
        public FlowState From { get; }

        /// <summary>
        /// The state transitioning to.
        /// </summary>
        public FlowState To { get; }

        /// <summary>
        /// Optional reason for the transition.
        /// </summary>
        public string? Reason { get; }

        /// <summary>
        /// When the transition occurred.
        /// </summary>
        public DateTime Timestamp { get; }

        public override string ToString()
        {
            var reasonPart = string.IsNullOrWhiteSpace(Reason) ? "" : $" ({Reason})";
            return $"{From} → {To}{reasonPart} at {Timestamp:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
