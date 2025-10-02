using Alma.Flows.Core.Description.Descriptors;

namespace Alma.Flows.Definitions
{
    public class ParameterDefinition
    {
        public required string Name { get; set; }
        public required string ValueType { get; set; }
        public required string ValueString { get; set; }

        public static ParameterDefinition Create(ParameterDescriptor descriptor)
        {
            return new ParameterDefinition
            {
                Name = descriptor.Name,
                ValueType = descriptor.ValueType,
                ValueString = string.Empty
            };
        }
    }
}
