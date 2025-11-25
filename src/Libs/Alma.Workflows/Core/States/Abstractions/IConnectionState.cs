using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Abstractions
{
    public interface IConnectionState : IStateComponent
    {
        ICollection<ExecutedConnection> AsCollection();

        void Add(ExecutedConnection executedConnection);
    }
}