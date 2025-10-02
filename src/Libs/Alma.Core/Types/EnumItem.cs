namespace Alma.Core.Types
{
    public class EnumItem
    {
        public required string Name { get; set; }
        public required string Value { get; set; }
    }

    public class EnumItem<TValue>
    {
        public required string Name { get; set; }
        public required TValue Value { get; set; }
    }
}