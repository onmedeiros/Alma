using Alma.Flows.Core.Abstractions;
using Alma.Flows.Core.Description.Descriptors;

namespace Alma.Flows.Definitions
{
    public class ActivityDefinition : IParameterizableDefinition
    {
        /// <summary>
        /// Identifier.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Discriminator.
        /// </summary>
        public string? Discriminator { get; set; }

        /// <summary>
        /// Last date update.
        /// </summary>
        public DateTime LastUpdate { get; set; } = DateTime.Now;

        /// <summary>
        /// Identifier name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Activity namespace.
        /// </summary>
        public required string Namespace { get; set; }

        /// <summary>
        /// Full qualified activity name (namespace + name).
        /// </summary>
        public required string FullName { get; set; }

        /// <summary>
        /// Custom display name defined by user for the activity.
        /// </summary>
        public string? CustomDisplayName { get; set; }

        /// <summary>
        /// Activity type. Can be Activity or Flow.
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// Type name of the activity class.
        /// </summary>
        public required string TypeName { get; set; }

        /// <summary>
        /// Parameter Definitions of the activity.
        /// </summary>
        public ICollection<ParameterDefinition> Parameters { get; set; } = [];

        /// <summary>
        /// Approval and Check Definitions of the activity.
        /// </summary>
        public ICollection<ApprovalAndCheckDefinition> ApprovalAndChecks { get; set; } = [];

        /// <summary>
        /// Metadata of the activity.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];

        public void AddApprovalAndCheck(ApprovalAndCheckDescriptor approvalAndCheckDescriptor)
        {
            var definition = ApprovalAndCheckDefinition.Create(approvalAndCheckDescriptor);

            definition.CustomName = approvalAndCheckDescriptor.DisplayName;

            ApprovalAndChecks.Add(definition);
        }

        public void RemoveApprovalAndCheck(string id)
        {
            var item = ApprovalAndChecks.FirstOrDefault(x => x.Id == id);

            if (item is not null)
                ApprovalAndChecks.Remove(item);
        }

        public static FlowDefinition Create(ActivityDescriptor descriptor)
        {
            return new FlowDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = descriptor.Name,
                Namespace = descriptor.Namespace,
                FullName = descriptor.FullName,
                Type = "Activity",
                TypeName = descriptor.TypeName,
                Parameters = descriptor.Parameters.Select(ParameterDefinition.Create).ToList()
            };
        }

        public ParameterDefinition GetParameterDefinition(string name)
        {
            return Parameters.First(x => x.Name == name);
        }

        public string? GetParameterValue(string name)
        {
            var parameter = Parameters.FirstOrDefault(x => x.Name == name);
            return parameter?.ValueString;
        }

        [Obsolete("Use SetParameterValue with ParameterDescriptor instead.")]
        public void SetParameterValue(string name, string value)
        {
            var parameter = Parameters.FirstOrDefault(x => x.Name == name);

            if (parameter is null)
            {
                throw new ArgumentException($"Parameter {name} not found.");
            }

            SetParameterValue(parameter, value);
        }

        public void SetParameterValue(ParameterDescriptor descriptor, string value)
        {
            var parameter = Parameters.FirstOrDefault(x => x.Name == descriptor.Name);

            if (parameter is null)
            {
                parameter = new ParameterDefinition
                {
                    Name = descriptor.Name,
                    ValueString = value,
                    ValueType = descriptor.Type.ToString()
                };

                Parameters.Add(parameter);
            }

            SetParameterValue(parameter, value);
        }

        public void SetParameterValue(ParameterDefinition definition, string value)
        {
            definition.ValueString = value;
        }
    }
}