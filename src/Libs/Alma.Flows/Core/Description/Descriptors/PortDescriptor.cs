using Alma.Flows.Core.Activities.Base;

namespace Alma.Flows.Core.Description.Descriptors
{
    public class PortDescriptor
    {
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required PortType Type { get; set; }
        public Type? DataType { get; set; }
        public string? DataTypeName { get; set; }
        public ICollection<object> Attributes { get; set; } = [];
    }
}
