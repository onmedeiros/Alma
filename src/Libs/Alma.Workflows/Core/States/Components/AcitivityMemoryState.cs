using Alma.Workflows.Core.ApprovalsAndChecks.Models;

namespace Alma.Workflows.Core.States.Components
{
    public interface IActivityMemoryState
    {
        public ValueTask SetValue(object? value);

        public ValueTask<object?> GetValue();

        public ValueTask<T?> GetValue<T>();
    }

    public class ActivityMemoryState : IActivityMemoryState
    {
        public ValueTask<object?> GetValue()
        {
            throw new NotImplementedException();
        }

        public ValueTask<T?> GetValue<T>()
        {
            throw new NotImplementedException();
        }

        public ValueTask SetValue(object? value)
        {
            throw new NotImplementedException();
        }
    }
}