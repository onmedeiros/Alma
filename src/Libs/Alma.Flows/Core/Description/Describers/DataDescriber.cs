using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Description.Descriptors;
using System.Reflection;

namespace Alma.Flows.Core.Description.Describers
{
    public static class DataDescriber
    {
        public static DataDescriptor Describe(PropertyInfo propertyInfo)
        {
            Type propertyType = propertyInfo.PropertyType;

            if (!propertyType.IsGenericType || !propertyType.GetGenericTypeDefinition().IsAssignableTo(typeof(Data<>)))
                throw new Exception("Property must be of type Data<T>");

            var valueType = propertyType.GetGenericArguments()[0];

            return new DataDescriptor
            {
                Name = propertyInfo.Name,
                Type = valueType
            };
        }
    }
}
