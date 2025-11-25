using Alma.Workflows.Core.ApprovalsAndChecks.Enums;
using Alma.Workflows.Core.ApprovalsAndChecks.Interfaces;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Options;
using Alma.Workflows.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners
{
    /// <summary>
    /// Responsible for resolving approvals and checks within a flow.
    /// </summary>
    public class ApprovalAndCheckResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ApprovalAndCheckResolver> _logger;
        private readonly IParameterSetter _parameterSetter;
        private readonly IApprovalAndCheck _approval;
        private readonly ActivityExecutionContext _context;
        private readonly ExecutionOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApprovalAndCheckResolver"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        /// <param name="approvalAndCheck">The approval and check to be resolved.</param>
        /// <param name="options">The execution options.</param>
        public ApprovalAndCheckResolver(IServiceProvider serviceProvider, IApprovalAndCheck approvalAndCheck, ExecutionOptions options)
        {
            _serviceProvider = serviceProvider;
            _approval = approvalAndCheck;
            _logger = serviceProvider.GetRequiredService<ILogger<ApprovalAndCheckResolver>>();
            _parameterSetter = serviceProvider.GetRequiredService<IParameterSetter>();
            _options = options;
            _context = new ActivityExecutionContext(_serviceProvider, options);
        }

        /// <summary>
        /// Resolves the approval and check asynchronously.
        /// </summary>
        /// <returns>The result of the approval and check resolution.</returns>
        public async Task<ApprovalAndCheckResult> Resolve()
        {
            var currentState = _context.State.Approvals.Get(_approval.Id);

            if (currentState is null)
            {
                currentState = new ApprovalStateData
                {
                    Id = _approval.Id,
                    FullName = _approval.Descriptor.FullName,
                    Status = ApprovalAndCheckStatus.Pending,
                    ParentActivityId = _approval.ParentActivity?.Id
                };

                _context.State.Approvals.Add(currentState);
            }
            else if (currentState.Status != ApprovalAndCheckStatus.Pending)
            {
                return new ApprovalAndCheckResult
                {
                    Status = currentState.Status
                };
            }

            var result = await _approval.Resolve(_context);

            currentState.Status = result.Status;

            return result;
        }
    }
}