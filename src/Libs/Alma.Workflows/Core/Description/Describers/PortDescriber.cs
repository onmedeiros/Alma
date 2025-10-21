using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.CustomActivities.Entities;
using Alma.Workflows.Core.Description.Descriptors;
using Alma.Core.Extensions;
using System.Reflection;

namespace Alma.Workflows.Core.Description.Describers
{
    public static class PortDescriber
    {
        public static PortDescriptor Describe(PropertyInfo property)
        {
            if (!property.PropertyType.IsAssignableFrom(typeof(Port)))
                throw new Exception("Property must be of type Port");

            var attributes = property.GetCustomAttributes(true);
            var portAttribute = attributes.OfType<PortAttribute>().FirstOrDefault();

            var descriptor = new PortDescriptor
            {
                Name = property.Name,
                DisplayName = portAttribute?.DisplayName.IsNullOrEmpty(property.Name)
                    ?? throw new Exception("Port name is not defined."),
                Type = portAttribute?.Type ?? PortType.Input,
                DataType = portAttribute?.DataType,
                DataTypeName = portAttribute?.DataType?.FullName,
                Attributes = attributes
            };

            return descriptor;
        }

        public static PortDescriptor Describe(CustomActivityPort port)
        {
            return new PortDescriptor
            {
                Name = port.Name,
                DisplayName = port.DisplayName,
                Type = port.Type
            };
        }
    }
}