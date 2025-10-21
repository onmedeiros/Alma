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
        private readonly IApprovalAndCheck _approvalAndCheck;
        private readonly ActivityExecutionContext _context;
        private readonly ExecutionOptions _options;
        private readonly ExecutionState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApprovalAndCheckResolver"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        /// <param name="approvalAndCheck">The approval and check to be resolved.</param>
        /// <param name="state">The current execution state.</param>
        /// <param name="options">The execution options.</param>
        public ApprovalAndCheckResolver(IServiceProvider serviceProvider, IApprovalAndCheck approvalAndCheck, ExecutionState state, ExecutionOptions options)
        {
            _serviceProvider = serviceProvider;
            _approvalAndCheck = approvalAndCheck;
            _logger = serviceProvider.GetRequiredService<ILogger<ApprovalAndCheckResolver>>();
            _parameterSetter = serviceProvider.GetRequiredService<IParameterSetter>();
            _state = state;
            _options = options;
            _context = new ActivityExecutionContext(_serviceProvider, state, options);
        }

        /// <summary>
        /// Resolves the approval and check asynchronously.
        /// </summary>
        /// <returns>The result of the approval and check resolution.</returns>
        public async Task<ApprovalAndCheckResult> Resolve()
        {
            var currentState = _context.State.ApprovalAndChecks.FirstOrDefault(x => x.Id == _approvalAndCheck.Id);

            if (currentState is null)
            {
                currentState = new ApprovalAndCheckState
                {
                    Id = _approvalAndCheck.Id,
                    FullName = _approvalAndCheck.Descriptor.FullName,
                    Status = ApprovalAndCheckStatus.Pending,
                    ParentActivityId = _approvalAndCheck.ParentActivity?.Id
                };

                _context.State.ApprovalAndChecks.Add(currentState);
            }
            else if (currentState.Status != ApprovalAndCheckStatus.Pending)
            {
                return new ApprovalAndCheckResult
                {
                    Status = currentState.Status
                };
            }

            // _parameterSetter.SetParameters(_context, _approvalAndCheck);

            var result = await _approvalAndCheck.Resolve(_context);

            currentState.Status = result.Status;

            return result;
        }
    }
}