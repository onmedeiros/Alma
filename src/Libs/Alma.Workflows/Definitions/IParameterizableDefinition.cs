using Alma.Workflows.Core.Description.Descriptors;
using Alma.Workflows.Definitions;

namespace Alma.Workflows.Core.Abstractions
{
    public interface IParameterizableDefinition
    {
        ICollection<ParameterDefinition> Parameters { get; set; }

        string? GetParameterValue(string name);

        [Obsolete("Use SetParameterValue with ParameterDescriptor instead")]
        void SetParameterValue(string name, string value);

        void SetParameterValue(ParameterDescriptor descriptor, string value);

        ParameterDefinition GetParameterDefinition(string name);
    }
}