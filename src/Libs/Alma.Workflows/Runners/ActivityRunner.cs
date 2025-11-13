using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Enums;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.ApprovalsAndChecks.Enums;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Enums;
using Alma.Workflows.Options;
using Alma.Workflows.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners
{
    public class ActivityRunner
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ActivityRunner> _logger;
        private readonly IParameterSetter _parameterSetter;
        private readonly IDataSetter _dataSetter;
        private readonly IApprovalAndCheckResolverFactory _approvalAndCheckResolverFactory;
        private readonly IActivity _activity;

        public bool RequireInteraction => _activity.Descriptor.RequireInteraction || Context.Options.ExecutionMode == Core.InstanceExecutions.Enums.InstanceExecutionMode.StepByStep;

        public ActivityExecutionContext Context { get; private set; }

        public ActivityRunner(IServiceProvider serviceProvider, IActivity activity, ExecutionState state, ExecutionOptions options)
        {
            _serviceProvider = serviceProvider;
            _activity = activity;
            _logger = serviceProvider.GetRequiredService<ILogger<ActivityRunner>>();
            _parameterSetter = serviceProvider.GetRequiredService<IParameterSetter>();
            _dataSetter = serviceProvider.GetRequiredService<IDataSetter>();
            _approvalAndCheckResolverFactory = serviceProvider.GetRequiredService<IApprovalAndCheckResolverFactory>();

            Context = new ActivityExecutionContext(_serviceProvider, state, options);
            Context.Id = activity.Id;

            // Load data from state
            _dataSetter.LoadData(Context.State, _activity);
        }

        /// <summary>
        /// Executes the activity if all validation checks pass.
        /// </summary>
        public async Task<ActivityExecutionResult> ExecuteAsync()
        {
            // First validate if the activity can be executed
            var validationResult = await ValidateActivityAsync();

            _logger.LogDebug("Activity {ActivityId} validation result: CanExecute={CanExecute}, " +
                            "ReadinessStatus={ReadinessStatus}, ApprovalStatus={ApprovalStatus}",
                            _activity.Id, validationResult.CanExecute,
                            validationResult.ReadinessStatus, validationResult.ApprovalStatus);

            // Create execution result from validation result
            var executionResult = new ActivityExecutionResult
            {
                ExecutionStatus = validationResult.ReadinessStatus,
                ExecutionStatusDetails = validationResult.ReadinessDetails,
                ApprovalAndCheckStatus = validationResult.ApprovalStatus
            };

            // If validation failed, return without executing
            if (!validationResult.CanExecute)
            {
                _logger.LogInformation("Activity {ActivityId} cannot be executed: ReadinessStatus={ReadinessStatus}, " +
                                      "ApprovalStatus={ApprovalStatus}, Details={Details}",
                                      _activity.Id, validationResult.ReadinessStatus,
                                      validationResult.ApprovalStatus, validationResult.ReadinessDetails);
                return executionResult;
            }

            // Execute the activity
            return await ExecuteActivityAsync(executionResult);
        }

        /// <summary>
        /// Validates if the activity is ready to execute and has all required approvals.
        /// </summary>
        public async Task<ActivityValidationResult> ValidateActivityAsync()
        {
            // Run before execution steps
            var beforeExecutionStatus = await RunBeforeExecutionSteps();

            if (beforeExecutionStatus != ActivityStepStatus.Completed)
            {
                return ActivityValidationResult.NotReady("Before execution steps failed", ApprovalAndCheckStatus.Pending);
            }

            // Check if activity is ready to execute
            var isReadyResult = await CheckIsReadyAsync();

            // Check approvals status
            var approvalStatus = await CheckApprovalsAsync();

            // Create validation result
            if (!isReadyResult.IsReady)
            {
                return ActivityValidationResult.NotReady(isReadyResult.Reason ?? "Activity is not ready", approvalStatus);
            }

            return ActivityValidationResult.Ready(approvalStatus);
        }

        /// <summary>
        /// Executes the activity after all validations have passed.
        /// </summary>
        private async Task<ActivityExecutionResult> ExecuteActivityAsync(ActivityExecutionResult executionResult)
        {
            try
            {
                _logger.LogInformation("Executing activity {ActivityId} ({DisplayName})",
                    _activity.Id, _activity.DisplayName);

                // Reset all port execution states before executing
                ResetPortExecutionStates();

                // _parameterSetter.SetParameters(Context, _activity);
                await _activity.ExecuteAsync(Context);

                var executedPorts = _activity.GetPorts()
                    .Where(x => x is not null && x.Executed);

                // Update data in state
                _dataSetter.UpdateData(Context.State, _activity);

                executionResult.ExecutedPorts = executedPorts;
                executionResult.ExecutionStatus = ActivityExecutionStatus.Completed;

                _logger.LogInformation("Activity {ActivityId} executed successfully", _activity.Id);
                return executionResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute activity {ActivityId}", _activity.Id);
                executionResult.ExecutionStatus = ActivityExecutionStatus.Failed;
                executionResult.ExecutionStatusDetails = ex.Message;

                return executionResult;
            }
        }

        /// <summary>
        /// Resets the execution state of all ports in the activity.
        /// This allows activities to be re-executed (important for loop activities).
        /// </summary>
        private void ResetPortExecutionStates()
        {
            foreach (var port in _activity.GetPorts())
            {
                port.Executed = false;
            }
        }

        /// <summary>
        /// Checks if the activity is ready to be executed.
        /// </summary>
        public ValueTask<IsReadyResult> CheckIsReadyAsync()
        {
            return _activity.IsReadyToExecuteAsync(Context);
        }

        /// <summary>
        /// Checks if the activity has all required approvals.
        /// </summary>
        public async ValueTask<ApprovalAndCheckStatus> CheckApprovalsAsync()
        {
            var approvalAndCheckResults = new List<ApprovalAndCheckResult>();

            foreach (var approvalAndCheck in _activity.ApprovalAndChecks)
            {
                var resolver = _approvalAndCheckResolverFactory.Create(approvalAndCheck, Context.State, Context.Options);
                approvalAndCheckResults.Add(await resolver.Resolve());
            }

            if (approvalAndCheckResults.Any(x => x.Status == ApprovalAndCheckStatus.Rejected))
                return ApprovalAndCheckStatus.Rejected;

            if (approvalAndCheckResults.All(x => x.Status == ApprovalAndCheckStatus.Approved))
                return ApprovalAndCheckStatus.Approved;

            return ApprovalAndCheckStatus.Pending;
        }

        public async ValueTask<ActivityStepStatus> RunBeforeExecutionSteps()
        {
            foreach (var step in _activity.BeforeExecutionSteps)
            {
                var status = await step.GetStatus(Context);

                if (status == ActivityStepStatus.Completed)
                    continue;

                if (status == ActivityStepStatus.Failed)
                    return ActivityStepStatus.Failed;

                status = await step.ExecuteAsync(Context);

                if (status != ActivityStepStatus.Completed)
                    return status;
            }

            return ActivityStepStatus.Completed;
        }
    }
}