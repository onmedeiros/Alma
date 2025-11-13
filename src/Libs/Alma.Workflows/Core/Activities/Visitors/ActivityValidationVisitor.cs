using Alma.Workflows.Core.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace Alma.Workflows.Core.Activities.Visitors
{
    /// <summary>
    /// Result of activity validation containing errors and warnings.
    /// </summary>
    public class ActivityValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; } = new();
        public List<string> Warnings { get; } = new();

        public void AddError(string error) => Errors.Add(error);
        public void AddWarning(string warning) => Warnings.Add(warning);

        public override string ToString()
        {
            if (IsValid)
                return "Validation passed";

            return $"Validation failed with {Errors.Count} error(s) and {Warnings.Count} warning(s)";
        }
    }

    /// <summary>
    /// Visitor that validates an activity's configuration.
    /// Checks for required parameters, valid connections, etc.
    /// </summary>
    public class ActivityValidationVisitor : IActivityVisitor<ActivityValidationResult>
    {
        public ActivityValidationResult Visit(IActivity activity)
        {
            var result = new ActivityValidationResult();

            // Validate basic properties
            ValidateBasicProperties(activity, result);

            // Validate parameters
            ValidateParameters(activity, result);

            // Validate ports
            ValidatePorts(activity, result);

            // Validate steps
            ValidateSteps(activity, result);

            return result;
        }

        private void ValidateBasicProperties(IActivity activity, ActivityValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(activity.Id))
                result.AddError("Activity Id is required");

            if (activity.Descriptor == null)
                result.AddError("Activity Descriptor is missing");
        }

        private void ValidateParameters(IActivity activity, ActivityValidationResult result)
        {
            if (activity.Descriptor?.Parameters == null)
                return;

            foreach (var paramDescriptor in activity.Descriptor.Parameters)
            {
                // Validate that parameter exists
                var value = activity.GetParameterValueAsString(paramDescriptor.Name);
                if (string.IsNullOrWhiteSpace(value))
                {
                    result.AddWarning($"Parameter '{paramDescriptor.Name}' is empty");
                }
            }
        }

        private void ValidatePorts(IActivity activity, ActivityValidationResult result)
        {
            var ports = activity.GetPorts().ToList();
            
            if (!ports.Any() && activity.Descriptor?.Ports.Any() == true)
            {
                result.AddWarning("Activity descriptor defines ports but none are instantiated");
            }

            foreach (var port in ports)
            {
                if (port.Descriptor == null)
                {
                    result.AddWarning($"Port has no descriptor");
                }
            }
        }

        private void ValidateSteps(IActivity activity, ActivityValidationResult result)
        {
            if (activity.BeforeExecutionSteps == null)
            {
                result.AddWarning("BeforeExecutionSteps collection is null");
            }

            if (activity.AfterExecutionSteps == null)
            {
                result.AddWarning("AfterExecutionSteps collection is null");
            }
        }
    }
}
