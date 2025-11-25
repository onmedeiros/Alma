using Alma.Workflows.Enums;
using Alma.Workflows.Models.Activities;

namespace Alma.Workflows.Core.States.Abstractions
{
    public interface ILogState : IStateComponent
    {
        void Add(string message);

        void Add(string message, LogSeverity level);

        void Add(string message, LogSeverity severity, DateTime timestamp);

        IReadOnlyCollection<LogModel> AsCollection();
    }
}