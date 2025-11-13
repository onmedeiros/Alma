namespace Alma.Workflows.Core.States.Observers
{
    /// <summary>
    /// Represents a state change event.
    /// </summary>
    public class StateChangeEvent
    {
        public StateChangeEvent(
            string changeType, 
            string? target = null, 
            object? oldValue = null, 
            object? newValue = null)
        {
            ChangeType = changeType;
            Target = target;
            OldValue = oldValue;
            NewValue = newValue;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Type of change (e.g., "VariableSet", "ParameterChanged", "StateTransition").
        /// </summary>
        public string ChangeType { get; }

        /// <summary>
        /// Target of the change (e.g., variable name, parameter name).
        /// </summary>
        public string? Target { get; }

        /// <summary>
        /// Old value before the change.
        /// </summary>
        public object? OldValue { get; }

        /// <summary>
        /// New value after the change.
        /// </summary>
        public object? NewValue { get; }

        /// <summary>
        /// When the change occurred.
        /// </summary>
        public DateTime Timestamp { get; }

        public override string ToString()
        {
            var targetPart = !string.IsNullOrWhiteSpace(Target) ? $" ({Target})" : "";
            return $"{ChangeType}{targetPart}: {OldValue} → {NewValue} at {Timestamp:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
