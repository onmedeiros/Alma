using Alma.Flows.Core.Contexts;
using Alma.Flows.Core.Description.Descriptors;
using System.Reflection;

namespace Alma.Flows.Core.Abstractions
{
    public interface IParameterizable
    {
        ICollection<ParameterDescriptor> ParameterDescriptors { get; }

        PropertyInfo GetParameterProperty(string name);

        TValue? GetParameterValue<TValue>(string name, ActivityExecutionContext context);

        string? GetParameterValueAsString(string name);

        void SetParameterValue(string name, object? value);
    }
}