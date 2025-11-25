using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Abstractions
{
    public interface IQueueState : IStateComponent
    {
        IReadOnlyCollection<QueueItem> AsCollection();

        void Add(QueueItem item);
    }
}