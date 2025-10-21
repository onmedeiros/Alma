namespace Alma.Modules.Widgets.Options
{
    public class WidgetOptions
    {
        public required string Name { get; set; }
        public int Width { get; set; } = 1;
        public int Height { get; set; } = 1;
        public int MaxWidth { get; set; } = 1;
        public int MaxHeight { get; set; } = 1;
        public string? Container { get; set; }
    }
}