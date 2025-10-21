namespace Alma.Modules.Widgets.Models
{
    public class WidgetDescriptor
    {
        public required Type Type { get; set; }
        public required string Name { get; set; }
        public string? Container { get; set; }
        public required int Width { get; set; }
        public required int Height { get; set; }
        public required int MaxWidth { get; set; }
        public required int MaxHeight { get; set; }
        public required int MinWidth { get; set; }
        public required int MinHeight { get; set; }
    }
}