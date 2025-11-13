namespace Alma.Workflows.Core.States.Observers
{
    /// <summary>
    /// Subject that can be observed for state changes.
    /// </summary>
    public interface IStateSubject
    {
        /// <summary>
        /// Attaches an observer.
        /// </summary>
        void Attach(IStateObserver observer);

        /// <summary>
        /// Detaches an observer.
        /// </summary>
        void Detach(IStateObserver observer);

        /// <summary>
        /// Notifies all observers of a state change.
        /// </summary>
        void NotifyObservers(StateChangeEvent changeEvent);
    }
}
