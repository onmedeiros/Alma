namespace Alma.Core.Modules
{
    public class ModuleDescriptor
    {
        public required string Name { get; set; }
        public required ModuleCategory Category { get; set; }
        public required string DisplayName { get; set; }
        public int Order { get; set; } = int.MaxValue;
        public string? Icon { get; set; }
        public required IEnumerable<ModuleMenuItem> Menu { get; set; }
    }
}
