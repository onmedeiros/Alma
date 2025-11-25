using Alma.Workflows.Core.States.Abstractions;
using Alma.Workflows.Core.States.Data;
using System.Diagnostics.CodeAnalysis;

namespace Alma.Workflows.Core.States.Components
{
    public abstract class StateComponent : IStateComponent
    {
        protected StateData? StateData { get; private set; }

        public virtual void Initialize(StateData data)
        {
            StateData = data;
        }

        public virtual void EnsureInitialized()
        {
            if (StateData is null)
            {
                throw new InvalidOperationException("StateComponent is not initialized. Call Initialize() before using the component.");
            }
        }
    }
}