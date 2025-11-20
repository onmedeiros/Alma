using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.States.Abstractions;
using Alma.Workflows.Enums;
using Alma.Workflows.Models;
using Alma.Workflows.Models.Activities;

namespace Alma.Workflows.States
{
    public class ExecutionState : IExecutionState
    {
        private Dictionary<string, object>? _templateVariables = null;

        public ExecutionQueue Queue { get; set; } = new ExecutionQueue();

        public Dictionary<string, Dictionary<string, object?>> ActivityData { get; set; } = [];

        public Dictionary<string, object?> Parameters { get; set; } = [];

        public Dictionary<string, ValueObject> Variables { get; set; } = [];

        public ICollection<ApprovalAndCheckState> ApprovalAndChecks { get; set; } = [];

        public ICollection<ExecutedConnection> ExecutedConnections { get; set; } = [];

        public ICollection<ExecutionHistory> History { get; set; } = [];

        public ICollection<LogModel> Logs = [];

        public ExecutionStatus Status => GetExecutionStatus();

        public ExecutionStatus GetExecutionStatus()
        {
            if (Queue.Any(x => x.ExecutionStatus == ActivityExecutionStatus.Failed))
                return ExecutionStatus.Failed;

            if (Queue.Any(x => x.ExecutionStatus == ActivityExecutionStatus.Ready))
                return ExecutionStatus.Executing;

            if (Queue.Any(x => x.ExecutionStatus == ActivityExecutionStatus.Waiting) || Queue.Any(x => x.ExecutionStatus == ActivityExecutionStatus.Pending))
                return ExecutionStatus.Waiting;

            return ExecutionStatus.Completed;
        }

        public void SetVariable(string name, object? value)
        {
            if (Variables.ContainsKey(name))
            {
                Variables.Remove(name);
            }

            Variables.Add(name, new ValueObject(value));
        }

        public Dictionary<string, object> GetTemplateVariables()
        {
            if (_templateVariables is null)
            {
                _templateVariables = new Dictionary<string, object>();
                _templateVariables.Add("_parameter", null!);
                _templateVariables.Add("_variable", null!);
            }

            _templateVariables["_parameter"] = Parameters;
            _templateVariables["_variable"] = Variables.ToDictionary(x => x.Key, x => x.Value.Value);

            return _templateVariables;
        }

        public ActivityExecutionStatus GetActivityExecutionStatus(string activityId)
        {
            var item = Queue.FirstOrDefault(x => x.ActivityId == activityId);

            if (item is null)
                return ActivityExecutionStatus.Pending;

            return item.ExecutionStatus;
        }

        public Dictionary<string, object?> GetActivityData(string activityId)
        {
            if (!ActivityData.TryGetValue(activityId, out Dictionary<string, object?>? value))
            {
                value = [];
                ActivityData.Add(activityId, value);
            }

            return value;
        }

        public void SetActivityData(string activityId, Dictionary<string, object?> data)
        {
            ActivityData.Remove(activityId);
            ActivityData.Add(activityId, data);
        }

        public void Log(string? message)
        {
            Log(message, LogSeverity.Information);
        }

        public void Log(string? message, LogSeverity? severity)
        {
            Logs.Add(new LogModel
            {
                Message = message ?? string.Empty,
                Severity = severity ?? LogSeverity.Information
            });
        }
    }
}