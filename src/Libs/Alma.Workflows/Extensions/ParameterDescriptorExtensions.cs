using Alma.Workflows.Core.Description.Descriptors;
using System.Diagnostics.CodeAnalysis;

namespace Alma.Workflows.Extensions
{
    public static class ParameterDescriptorExtensions
    {
        public static bool ContainsAttribute(this ParameterDescriptor parameterDescriptor, Type attributeType)
        {
            return parameterDescriptor.Attributes.Any(x => x.GetType() == attributeType);
        }

        public static bool TryGetAttribute<T>(this ParameterDescriptor parameterDescriptor, [NotNullWhen(true)] out T? attribute)
        {
            attribute = parameterDescriptor.Attributes.OfType<T>().FirstOrDefault();
            return attribute != null;
        }
    }
}
