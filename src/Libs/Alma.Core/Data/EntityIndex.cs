namespace Alma.Core.Data
{
    public class EntityIndex
    {
        public required Type EntityType { get; set; }
        public required string[] Properties { get; set; }
        public required EntityIndexType Type { get; set; }
        public string? Name { get; set; }
    }
}