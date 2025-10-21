using Alma.Workflows.Core.ApprovalsAndChecks.Interfaces;
using Alma.Workflows.Options;
using Alma.Workflows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners
{
    /// <summary>
    /// Factory interface for creating instances of <see cref="ApprovalAndCheckResolver"/>.
    /// </summary>
    public interface IApprovalAndCheckResolverFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="ApprovalAndCheckResolver"/>.
        /// </summary>
        /// <param name="approvalAndCheck">The approval and check to be resolved.</param>
        /// <param name="state">The current execution state.</param>
        /// <param name="options">The execution options.</param>
        /// <returns>A new instance of <see cref="ApprovalAndCheckResolver"/>.</returns>
        ApprovalAndCheckResolver Create(IApprovalAndCheck approvalAndCheck, ExecutionState state, ExecutionOptions options);
    }

    /// <summary>
    /// Factory class for creating instances of <see cref="ApprovalAndCheckResolver"/>.
    /// </summary>
    public class ApprovalAndCheckResolverFactory : IApprovalAndCheckResolverFactory
    {
        private readonly ILogger<ApprovalAndCheckResolverFactory> _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApprovalAndCheckResolverFactory"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        public ApprovalAndCheckResolverFactory(ILogger<ApprovalAndCheckResolverFactory> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public ApprovalAndCheckResolver Create(IApprovalAndCheck approvalAndCheck, ExecutionState state, ExecutionOptions options)
        {
            return new ApprovalAndCheckResolver(_serviceProvider, approvalAndCheck, state, options);
        }
    }
}
