using Alma.Core.Attributes;
using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.ApprovalsAndChecks.Enums;
using Alma.Workflows.Enums;

namespace Alma.Workflows.States
{
    public class QueueItem
    {
        public string Id { get; set; }

        public int Sequential { get; set; }

        public string ActivityId { get; set; }

        public ICollection<string> ExecutedConnectionIds { get; set; } = [];

        public ActivityExecutionStatus ExecutionStatus { get; set; } = ActivityExecutionStatus.Pending;

        public string? ExecutionStatusReason { get; set; }

        public ApprovalAndCheckStatus ApprovalAndCheckStatus { get; set; } = ApprovalAndCheckStatus.Pending;

        public bool CanExecute =>
            ExecutionStatus == ActivityExecutionStatus.Ready;

        #region Navigations

        [Navigation]
        public IActivity Activity { get; set; }

        [Navigation]
        public ICollection<ExecutedConnection> ExecutedConnections { get; set; } = new List<ExecutedConnection>();

        #endregion

        public QueueItem(IActivity activity, int sequential)
        {
            Id = Guid.NewGuid().ToString();
            Sequential = sequential;
            ActivityId = activity.Id;
            Activity = activity;
        }

        public QueueItem(ExecutedConnection executedConnection, int sequential)
            : this(executedConnection.Source, sequential)
        {
            ExecutedConnectionIds.Add(executedConnection.Id);
            ExecutedConnections.Add(executedConnection);
        }
    }
}