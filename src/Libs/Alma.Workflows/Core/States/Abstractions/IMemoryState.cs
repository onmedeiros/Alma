using Alma.Workflows.Core.ApprovalsAndChecks.Models;

namespace Alma.Workflows.Core.States.Abstractions
{
    public interface IMemoryState : IStateComponent
    {
        void Set(string activityId, string key, object? value);

        ValueObject Get(string activityId, string key);

        T? Get<T>(string activityId, string key);
    }
}