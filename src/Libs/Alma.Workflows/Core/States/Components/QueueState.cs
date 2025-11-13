using Alma.Workflows.Enums;
using Alma.Workflows.Models;
using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Concrete implementation of queue state management.
    /// </summary>
    public class QueueState : IQueueState
    {
        private readonly ExecutionQueue _queue;
        private readonly List<ExecutedConnection> _executedConnections;

        public QueueState()
        {
            _queue = new ExecutionQueue();
            _executedConnections = new List<ExecutedConnection>();
        }

        public ExecutionQueue Queue => _queue;

        public ExecutionStatus GetExecutionStatus()
        {
            if (_queue.Any(x => x.ExecutionStatus == ActivityExecutionStatus.Failed))
                return ExecutionStatus.Failed;

            if (_queue.Any(x => x.ExecutionStatus == ActivityExecutionStatus.Ready))
                return ExecutionStatus.Executing;

            if (_queue.Any(x => x.ExecutionStatus == ActivityExecutionStatus.Waiting) || 
                _queue.Any(x => x.ExecutionStatus == ActivityExecutionStatus.Pending))
                return ExecutionStatus.Waiting;

            return ExecutionStatus.Completed;
        }

        public ActivityExecutionStatus GetActivityExecutionStatus(string activityId)
        {
            var item = _queue.FirstOrDefault(x => x.ActivityId == activityId);
            return item?.ExecutionStatus ?? ActivityExecutionStatus.Pending;
        }

        public ICollection<ExecutedConnection> GetExecutedConnections()
        {
            return _executedConnections.AsReadOnly();
        }

        public void AddExecutedConnection(ExecutedConnection connection)
        {
            _executedConnections.Add(connection);
        }

        public void ClearExecutedConnections()
        {
            _executedConnections.Clear();
        }
    }
}
