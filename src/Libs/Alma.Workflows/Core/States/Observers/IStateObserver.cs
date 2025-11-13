namespace Alma.Workflows.Core.States.Observers
{
    /// <summary>
    /// Observer that gets notified of state changes.
    /// </summary>
    public interface IStateObserver
    {
        /// <summary>
        /// Called when a state change occurs.
        /// </summary>
        void OnStateChanged(StateChangeEvent changeEvent);
    }
}
