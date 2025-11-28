using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.States.Abstractions;
using Alma.Workflows.Core.States.Data;
using Alma.Workflows.Enums;
using Alma.Workflows.Models.Activities;

namespace Alma.Workflows.States
{
    public class ExecutionState : IExecutionState
    {
        #region Template

        private Dictionary<string, object>? _templateDataCache;

        #endregion

        #region Data

        public StateData? StateData { get; private set; }

        #endregion

        #region State

        public IParameterState Parameters { get; init; }

        public IVariableState Variables { get; init; }

        public IMemoryState Memory { get; init; }

        public IQueueState Queue { get; init; }

        public IStepState Steps { get; init; }

        public IApprovalState Approvals { get; init; }

        public IConnectionState Connections { get; init; }

        public IHistoryState History { get; init; }

        public ILogState Logs { get; init; }

        #endregion

        public ExecutionStatus ExecutionStatus => GetExecutionStatus();

        public ExecutionState(IParameterState parameters, IVariableState variables, IMemoryState memory, IQueueState queue, IStepState steps, IApprovalState approvals, IConnectionState connections, IHistoryState history, ILogState logs)
        {
            Parameters = parameters;
            Variables = variables;
            Memory = memory;
            Queue = queue;
            Steps = steps;
            Approvals = approvals;
            Connections = connections;
            History = history;
            Logs = logs;
        }

        public void Initialize(StateData? data)
        {
            data ??= new();

            StateData = data;

            Parameters.Initialize(data);
            Variables.Initialize(data);
            Memory.Initialize(data);
            Queue.Initialize(data);
            Steps.Initialize(data);
            Approvals.Initialize(data);
            Connections.Initialize(data);
            History.Initialize(data);
            Logs.Initialize(data);
        }

        public Dictionary<string, object> AsTemplateData()
        {
            _templateDataCache ??= new Dictionary<string, object>
                {
                    { "_parameter", null! },
                    { "_variable", null! }
                };

            _templateDataCache["_parameter"] = Parameters.AsDictionary().ToDictionary(x => x.Key, x => x.Value.GetValue());
            _templateDataCache["_variable"] = Variables.AsDictionary().ToDictionary(x => x.Key, x => x.Value.GetValue());

            return _templateDataCache;
        }

        private ExecutionStatus GetExecutionStatus()
        {
            if (Queue.AsCollection().Any(x => x.ExecutionStatus == ActivityExecutionStatus.Failed))
                return ExecutionStatus.Failed;

            if (Queue.AsCollection().Any(x => x.ExecutionStatus == ActivityExecutionStatus.Ready))
                return ExecutionStatus.Executing;

            if (Queue.AsCollection().Any(x => x.ExecutionStatus == ActivityExecutionStatus.Waiting) || Queue.AsCollection().Any(x => x.ExecutionStatus == ActivityExecutionStatus.Pending))
                return ExecutionStatus.Waiting;

            return ExecutionStatus.Completed;
        }
    }
}