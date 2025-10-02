using Alma.Flows.Core.Activities.Base;

namespace Alma.Flows.Core.Activities.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PortAttribute : Attribute
    {
        public string? DisplayName { get; set; }
        public PortType Type { get; set; } = PortType.Input;
        public Type? DataType { get; set; }
    }
}
