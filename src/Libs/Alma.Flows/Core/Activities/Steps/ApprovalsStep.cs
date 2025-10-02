using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Activities.Enums;
using Alma.Flows.Core.ApprovalsAndChecks.Enums;
using Alma.Flows.Core.ApprovalsAndChecks.Models;
using Alma.Flows.Core.Contexts;
using Alma.Flows.Runners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Flows.Core.Activities.Steps
{
    public class ApprovalsStep : Step
    {
        private readonly ILogger<ApprovalsStep> _logger;

        public ApprovalsStep(ILogger<ApprovalsStep> logger)
        {
            _logger = logger;
        }

        public override async ValueTask<ActivityStepStatus> ExecuteAsync(ActivityExecutionContext context)
        {
            _logger.LogDebug("Executing ApprovalsStep for activity {ActivityId}", Activity.Id);

            var approvalResults = new List<ApprovalAndCheckResult>();
            var approvalResolverFactory = context.ServiceProvider.GetRequiredService<IApprovalAndCheckResolverFactory>();

            // Resolve approvals
            foreach (var approvalAndCheck in Activity.ApprovalAndChecks)
            {
                var resolver = approvalResolverFactory.Create(approvalAndCheck, context.State, context.Options);
                var result = await resolver.Resolve();
                approvalResults.Add(result);
            }

            // Check approval results
            if (approvalResults.Any(x => x.Status == ApprovalAndCheckStatus.Rejected))
            {
                _logger.LogDebug("Activity {ActivityId} has been rejected by approvals.", Activity.Id);
                return ActivityStepStatus.Failed;
            }

            if (approvalResults.All(x => x.Status == ApprovalAndCheckStatus.Approved))
            {
                _logger.LogDebug("Activity {ActivityId} has been approved by all approvals.", Activity.Id);
                return ActivityStepStatus.Completed;
            }

            _logger.LogDebug("Activity {ActivityId} has pending approvals.", Activity.Id);
            return ActivityStepStatus.Waiting;
        }
    }
}