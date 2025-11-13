using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Description.Descriptors;
using System.Text;

namespace Alma.Workflows.Core.Activities.Visitors
{
    /// <summary>
    /// Visitor that generates a textual description of an activity.
    /// Useful for debugging, logging, and documentation.
    /// </summary>
    public class ActivityDescriptionVisitor : IActivityVisitor<string>
    {
        private readonly bool _includeParameters;
        private readonly bool _includePorts;
        private readonly bool _includeData;

        public ActivityDescriptionVisitor(
            bool includeParameters = true,
            bool includePorts = true,
            bool includeData = false)
        {
            _includeParameters = includeParameters;
            _includePorts = includePorts;
            _includeData = includeData;
        }

        public string Visit(IActivity activity)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Activity: {activity.DisplayName} ({activity.Id})");
            sb.AppendLine($"Type: {activity.GetType().Name}");
            sb.AppendLine($"Descriptor: {activity.Descriptor?.Name ?? "N/A"}");

            if (_includeParameters && activity.Descriptor?.Parameters != null)
            {
                sb.AppendLine("\nParameters:");
                foreach (var param in activity.Descriptor.Parameters)
                {
                    var value = activity.GetParameterValueAsString(param.Name);
                    sb.AppendLine($"  - {param.Name} ({param.Type.Name}): {value ?? "null"}");
                }
            }

            if (_includePorts)
            {
                var ports = activity.GetPorts().ToList();
                if (ports.Any())
                {
                    sb.AppendLine("\nPorts:");
                    foreach (var port in ports)
                    {
                        sb.AppendLine($"  - {port.Descriptor?.Name ?? "Unknown"} ({port.Type})");
                    }
                }
            }

            if (_includeData)
            {
                // Data outputs would go here if descriptor had them
                // For now, just list that data outputs are available
                var dataProps = activity.GetType().GetProperties()
                    .Where(p => p.PropertyType.IsGenericType && 
                                p.PropertyType.GetGenericTypeDefinition() == typeof(Core.Activities.Base.Data<>));
                
                if (dataProps.Any())
                {
                    sb.AppendLine("\nData Properties:");
                    foreach (var dataProp in dataProps)
                    {
                        var value = activity.GetDataValue(dataProp.Name);
                        sb.AppendLine($"  - {dataProp.Name}: {value?.ToString() ?? "null"}");
                    }
                }
            }

            if (activity.ApprovalAndChecks.Any())
            {
                sb.AppendLine($"\nApprovals & Checks: {activity.ApprovalAndChecks.Count}");
            }

            if (activity.BeforeExecutionSteps.Any())
            {
                sb.AppendLine($"Before Execution Steps: {activity.BeforeExecutionSteps.Count}");
            }

            if (activity.AfterExecutionSteps.Any())
            {
                sb.AppendLine($"After Execution Steps: {activity.AfterExecutionSteps.Count}");
            }

            return sb.ToString();
        }
    }
}
