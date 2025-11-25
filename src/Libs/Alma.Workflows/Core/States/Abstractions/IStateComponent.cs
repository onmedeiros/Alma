using Alma.Workflows.Core.States.Data;

namespace Alma.Workflows.Core.States.Abstractions
{
    public interface IStateComponent
    {
        void Initialize(StateData data);
    }
}