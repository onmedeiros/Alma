using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.States.Components;
using Alma.Workflows.Core.States.Observers;
using Alma.Workflows.Enums;
using Alma.Workflows.Models;
using Alma.Workflows.Models.Activities;

namespace Alma.Workflows.States
{
    /// <summary>
    /// Manages execution state using separated state components.
    /// Refactored to follow Single Responsibility Principle.
    /// </summary>
    public class ExecutionState : IStateSubject
    {
        private readonly IQueueState _queueState;
        private readonly IVariableState _variableState;
        private readonly IParameterState _parameterState;
        private readonly IActivityDataState _activityDataState;
        private readonly IApprovalState _approvalState;
        private readonly IHistoryState _historyState;
        private readonly ILogState _logState;
        private readonly List<IStateObserver> _observers;
        private Dictionary<string, object>? _templateVariables = null;

        public ExecutionState()
        {
            _queueState = new QueueState();
            _variableState = new VariableState();
            _parameterState = new ParameterState();
            _activityDataState = new ActivityDataState();
            _approvalState = new ApprovalState();
            _historyState = new HistoryState();
            _logState = new LogState();
            _observers = new List<IStateObserver>();
        }

        // Backward compatibility properties - delegates to components
        public ExecutionQueue Queue 
        { 
            get => _queueState.Queue; 
            set { /* Setter kept for compatibility but not recommended */ } 
        }

        public Dictionary<string, Dictionary<string, object?>> ActivityData 
        { 
            get => new Dictionary<string, Dictionary<string, object?>>(_activityDataState.GetAll()); 
            set { /* Setter kept for compatibility but not recommended */ } 
        }

        // WORKAROUND: Return internal dictionary for compatibility with tests that modify it directly
        // This breaks encapsulation but maintains backward compatibility
        private Dictionary<string, object?>? _parametersCache = null;
        public Dictionary<string, object?> Parameters 
        {
            get
            {
                if (_parametersCache == null)
                {
                    _parametersCache = new Dictionary<string, object?>(_parameterState.GetAll());
                }
                else
                {
                    // Sync back any changes to the internal state
                    foreach (var kv in _parametersCache)
                    {
                        _parameterState.Set(kv.Key, kv.Value);
                    }
                }
                return _parametersCache;
            }
            set
            {
                _parametersCache = value;
                if (value != null)
                {
                    _parameterState.Clear();
                    foreach (var kv in value)
                    {
                        _parameterState.Set(kv.Key, kv.Value);
                    }
                }
            }
        }

        public Dictionary<string, ValueObject> Variables 
        { 
            get => new Dictionary<string, ValueObject>(_variableState.GetAll()); 
            set { /* Setter kept for compatibility but not recommended */ } 
        }

        public ICollection<ApprovalAndCheckState> ApprovalAndChecks 
        { 
            get => _approvalState.GetAll().ToList(); 
            set { /* Setter kept for compatibility but not recommended */ } 
        }

        // WORKAROUND: Return modifiable list for ExecutedConnections to maintain compatibility
        private List<ExecutedConnection>? _executedConnectionsCache = null;
        public ICollection<ExecutedConnection> ExecutedConnections 
        { 
            get
            {
                if (_executedConnectionsCache == null)
                {
                    _executedConnectionsCache = _queueState.GetExecutedConnections().ToList();
                }
                else
                {
                    // Sync back any additions to the internal state
                    var currentInternal = _queueState.GetExecutedConnections();
                    foreach (var conn in _executedConnectionsCache)
                    {
                        if (!currentInternal.Contains(conn))
                        {
                            _queueState.AddExecutedConnection(conn);
                        }
                    }
                }
                return _executedConnectionsCache;
            }
            set
            {
                _executedConnectionsCache = value?.ToList();
                if (value != null)
                {
                    _queueState.ClearExecutedConnections();
                    foreach (var conn in value)
                    {
                        _queueState.AddExecutedConnection(conn);
                    }
                }
            }
        }

        public ICollection<ExecutionHistory> History 
        { 
            get => _historyState.GetAll().ToList(); 
            set { /* Setter kept for compatibility but not recommended */ } 
        }

        public ICollection<LogModel> Logs => _logState.GetAll().ToList();

        // Delegated methods
        public ExecutionStatus Status => _queueState.GetExecutionStatus();

        public ExecutionStatus GetExecutionStatus() => _queueState.GetExecutionStatus();

        public void SetVariable(string name, object? value)
        {
            var oldValue = _variableState.Get(name);
            _variableState.Set(name, value);
            NotifyObservers(new StateChangeEvent("VariableSet", name, oldValue?.Value, value));
        }

        public Dictionary<string, object> GetTemplateVariables()
        {
            // Sync any changes from Parameters cache back to the state before returning
            if (_parametersCache != null)
            {
                _parameterState.Clear();
                foreach (var kv in _parametersCache)
                {
                    _parameterState.Set(kv.Key, kv.Value);
                }
            }

            if (_templateVariables is null)
            {
                _templateVariables = new Dictionary<string, object>
                {
                    { "_parameter", null! },
                    { "_variable", null! }
                };
            }

            _templateVariables["_parameter"] = new Dictionary<string, object?>(_parameterState.GetAll());
            _templateVariables["_variable"] = _variableState.GetAll().ToDictionary(x => x.Key, x => x.Value.Value);

            return _templateVariables;
        }

        public ActivityExecutionStatus GetActivityExecutionStatus(string activityId) 
            => _queueState.GetActivityExecutionStatus(activityId);

        public Dictionary<string, object?> GetActivityData(string activityId) 
            => _activityDataState.GetActivityData(activityId);

        public void SetActivityData(string activityId, Dictionary<string, object?> data) 
            => _activityDataState.SetActivityData(activityId, data);

        public void Log(string? message) => _logState.Log(message);

        public void Log(string? message, LogSeverity? severity) 
            => _logState.Log(message, severity ?? LogSeverity.Information);

        // Observer pattern implementation
        public void Attach(IStateObserver observer) => _observers.Add(observer);

        public void Detach(IStateObserver observer) => _observers.Remove(observer);

        public void NotifyObservers(StateChangeEvent changeEvent)
        {
            foreach (var observer in _observers)
            {
                observer.OnStateChanged(changeEvent);
            }
        }
    }
}