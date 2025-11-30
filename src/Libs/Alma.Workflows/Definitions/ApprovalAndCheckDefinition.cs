using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.Description.Descriptors;

namespace Alma.Workflows.Definitions
{
    public class ApprovalAndCheckDefinition : IParameterizableDefinition, IRenamable
    {
        public required string Id { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
        public string? CustomName { get; set; }
        public required string TypeName { get; set; }
        public ICollection<ParameterDefinition> Parameters { get; set; } = [];

        public static ApprovalAndCheckDefinition Create(ApprovalAndCheckDescriptor descriptor)
        {
            var now = DateTime.Now;

            return new ApprovalAndCheckDefinition
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = now,
                UpdatedAt = now,
                TypeName = descriptor.TypeName
            };
        }

        public string GetCustomName() => CustomName ?? string.Empty;

        public void SetCustomName(string name)
        {
            CustomName = name;
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
