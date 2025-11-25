using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Enums;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.ApprovalsAndChecks.Enums;
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
        private readonly IActivity _activity;

        public bool RequireInteraction => _activity.Descriptor.RequireInteraction || Context.Options.ExecutionMode == Core.InstanceExecutions.Enums.InstanceExecutionMode.StepByStep;

        public ActivityExecutionContext Context { get; private set; }

        public ActivityRunner(IServiceProvider serviceProvider, IActivity activity, ExecutionOptions options)
        {
            _serviceProvider = serviceProvider;
            _activity = activity;
            _logger = serviceProvider.GetRequiredService<ILogger<ActivityRunner>>();
            _parameterSetter = serviceProvider.GetRequiredService<IParameterSetter>();

            Context = new ActivityExecutionContext(_serviceProvider, options);
            Context.Id = activity.Id;
        }

        /// <summary>
        /// Executes the activity if all validation checks pass.
        /// </summary>
        public async Task<ActivityExecutionResult> ExecuteAsync()
        {
            // Run before execution steps (includes all validations via Steps)
            var beforeExecutionStatus = await RunBeforeExecutionSteps();

            _logger.LogDebug("Activity {ActivityId} before execution steps status: {Status}",
                            _activity.Id, beforeExecutionStatus);

            // Create execution result based on step status
            var executionResult = new ActivityExecutionResult
            {
                ExecutionStatus = ConvertStepStatusToActivityStatus(beforeExecutionStatus),
                ExecutionStatusDetails = GetStatusDetails(beforeExecutionStatus),
                ApprovalAndCheckStatus = ApprovalAndCheckStatus.Pending
            };

            // If steps didn't complete, return without executing
            if (beforeExecutionStatus != ActivityStepStatus.Completed)
            {
                _logger.LogInformation("Activity {ActivityId} cannot be executed. Step status: {Status}",
                                      _activity.Id, beforeExecutionStatus);
                return executionResult;
            }

            // Execute the activity
            return await ExecuteActivityAsync(executionResult);
        }

        /// <summary>
        /// Executes all before execution steps.
        /// Steps include all validations (readiness, approvals, etc).
        /// </summary>
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

        /// <summary>
        /// Executes the activity after all steps have completed.
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
        /// Converts ActivityStepStatus to ActivityExecutionStatus.
        /// </summary>
        private ActivityExecutionStatus ConvertStepStatusToActivityStatus(ActivityStepStatus stepStatus)
        {
            return stepStatus switch
            {
                ActivityStepStatus.Completed => ActivityExecutionStatus.Ready,
                ActivityStepStatus.Waiting => ActivityExecutionStatus.Waiting,
                ActivityStepStatus.Failed => ActivityExecutionStatus.Failed,
                ActivityStepStatus.Pending => ActivityExecutionStatus.Pending,
                _ => ActivityExecutionStatus.Pending
            };
        }

        /// <summary>
        /// Gets status details message based on step status.
        /// </summary>
        private string GetStatusDetails(ActivityStepStatus stepStatus)
        {
            return stepStatus switch
            {
                ActivityStepStatus.Waiting => "Waiting for steps to complete",
                ActivityStepStatus.Failed => "Steps failed",
                ActivityStepStatus.Pending => "Steps pending",
                _ => string.Empty
            };
        }
    }
}