namespace Alma.Modules.Widgets.Interop
{
    public class GridStackInteropEventArgs
    {
        public string EventName { get; set; } = string.Empty;
        public ICollection<GridStackInteropWidget> Widgets { get; set; } = [];
    }
}