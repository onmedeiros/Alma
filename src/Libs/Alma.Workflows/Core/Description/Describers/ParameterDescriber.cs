using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Common.Extensions;
using Alma.Workflows.Core.CustomActivities.Entities;
using Alma.Workflows.Core.Description.Descriptors;
using System.Reflection;

namespace Alma.Workflows.Core.Description.Describers
{
    public static class ParameterDescriber
    {
        public static ParameterDescriptor Describe(PropertyInfo property)
        {
            Type propertyType = property.PropertyType;

            if (!propertyType.IsGenericType || !propertyType.GetGenericTypeDefinition().IsAssignableTo(typeof(Parameter<>)))
                throw new Exception("Property must be of type Parameter<T>");

            var attributes = property.GetCustomAttributes();
            var inputAttribute = attributes.OfType<ActivityParameterAttribute>().FirstOrDefault();
            var valueType = propertyType.GetGenericArguments()[0];

            return new ParameterDescriptor
            {
                Name = property.Name,
                DisplayName = inputAttribute?.DisplayName ?? property.Name,
                DisplayValue = inputAttribute?.DisplayValue,
                ValueType = valueType.Name,
                Type = valueType,
                Attributes = (ICollection<object>)attributes
            };
        }

        public static ParameterDescriptor Describe(CustomActivityParameterTemplate parameter)
        {
            return new ParameterDescriptor
            {
                Name = parameter.Name,
                DisplayName = parameter.DisplayName,
                ValueType = parameter.Type.GetParameterTypeName(),
                Type = parameter.Type.GetParameterType()
            };
        }
    }
}