using Alma.Workflows.Core.ApprovalsAndChecks.Models;

namespace Alma.Workflows.Core.States.Abstractions
{
    public interface IVariableState : IStateComponent
    {
        void Set(string name, object? value);

        ValueObject? Get(string name);

        bool TryGet(string name, out ValueObject? value);

        T? GetValue<T>(string name);

        bool TryGetValue<T>(string name, out T? value);

        Dictionary<string, ValueObject> AsDictionary();

        Dictionary<string, object?> AsObjectDictionary();
    }
}