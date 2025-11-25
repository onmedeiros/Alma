using Alma.Workflows.Core.States.Data;
using Alma.Workflows.Enums;

namespace Alma.Workflows.Core.States.Abstractions
{
    public interface IExecutionState
    {
        StateData? StateData { get; }

        IParameterState Parameters { get; init; }
        IVariableState Variables { get; init; }
        IMemoryState Memory { get; init; }
        IQueueState Queue { get; init; }
        IStepState Steps { get; init; }
        IApprovalState Approvals { get; init; }
        IConnectionState Connections { get; init; }
        IHistoryState History { get; init; }
        ILogState Logs { get; init; }

        ExecutionStatus ExecutionStatus { get; }

        void Initialize(StateData data);

        Dictionary<string, object> AsTemplateData();
    }
}