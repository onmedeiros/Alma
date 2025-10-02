using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Alma.Flows.Design.Components.Nodes;

namespace Alma.Flows.Design.Components.Ports
{
    public class ActivityPortModel : PortModel
    {
        public ActivityPortType Type { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Color { get; set; }

        public ActivityPortModel(ActivityNodeModel parent, ActivityPortType type, PortAlignment alignment = PortAlignment.Right, Point? position = null, Size? size = null)
            : base(parent, alignment, position, size)
        {
            Type = type;
        }
    }
}
