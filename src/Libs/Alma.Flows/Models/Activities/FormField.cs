using Alma.Flows.Enums;

namespace Alma.Flows.Models.Activities
{
    public class FormField
    {
        public required string Name { get; set; }
        public string? Label { get; set; }
        public FieldType Type { get; set; }
        public string? Placeholder { get; set; }
        public bool Required { get; set; }
    }
}