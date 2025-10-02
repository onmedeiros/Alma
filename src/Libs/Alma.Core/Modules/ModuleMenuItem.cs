namespace Alma.Core.Modules
{
    public class ModuleMenuItem
    {
        public required string Path { get; set; }
        public required string DisplayName { get; set; }

        public bool Enabled { get; set; } = true;
    }
}