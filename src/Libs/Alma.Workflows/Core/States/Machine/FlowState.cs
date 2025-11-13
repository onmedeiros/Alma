namespace Alma.Workflows.Core.States.Machine
{
    /// <summary>
    /// Represents the possible states of a flow execution.
    /// </summary>
    public enum FlowState
    {
        /// <summary>
        /// Flow has not started execution yet.
        /// </summary>
        NotStarted,

        /// <summary>
        /// Flow is currently running.
        /// </summary>
        Running,

        /// <summary>
        /// Flow is waiting for external input or approval.
        /// </summary>
        Waiting,

        /// <summary>
        /// Flow execution has been paused.
        /// </summary>
        Paused,

        /// <summary>
        /// Flow execution completed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// Flow execution failed with errors.
        /// </summary>
        Failed,

        /// <summary>
        /// Flow execution was cancelled.
        /// </summary>
        Cancelled
    }
}
