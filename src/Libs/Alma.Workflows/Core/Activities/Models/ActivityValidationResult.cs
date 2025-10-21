using Alma.Workflows.Core.ApprovalsAndChecks.Enums;
using Alma.Workflows.Enums;

namespace Alma.Workflows.Core.Activities.Models
{
    /// <summary>
    /// Represents the result of validating an activity before execution.
    /// </summary>
    public class ActivityValidationResult
    {
        /// <summary>
        /// The readiness status of the activity.
        /// </summary>
        public ActivityExecutionStatus ReadinessStatus { get; set; } = ActivityExecutionStatus.Pending;

        /// <summary>
        /// Details about the readiness status, particularly useful for non-ready statuses.
        /// </summary>
        public string? ReadinessDetails { get; set; }

        /// <summary>
        /// The status of approvals and checks for the activity.
        /// </summary>
        public ApprovalAndCheckStatus ApprovalStatus { get; set; } = ApprovalAndCheckStatus.Pending;

        /// <summary>
        /// Indicates whether the activity can be executed based on readiness and approval status.
        /// </summary>
        public bool CanExecute => ReadinessStatus == ActivityExecutionStatus.Ready &&
                                 ApprovalStatus == ApprovalAndCheckStatus.Approved;

        /// <summary>
        /// Creates an ActivityValidationResult indicating an activity is ready to execute.
        /// </summary>
        public static ActivityValidationResult Ready(ApprovalAndCheckStatus approvalStatus) =>
            new()
            {
                ReadinessStatus = ActivityExecutionStatus.Ready,
                ApprovalStatus = approvalStatus
            };

        /// <summary>
        /// Creates an ActivityValidationResult indicating an activity is not ready to execute.
        /// </summary>
        public static ActivityValidationResult NotReady(string reason, ApprovalAndCheckStatus approvalStatus) =>
            new()
            {
                ReadinessStatus = ActivityExecutionStatus.Waiting,
                ReadinessDetails = reason,
                ApprovalStatus = approvalStatus
            };
    }
}