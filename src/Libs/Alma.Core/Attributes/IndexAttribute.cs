namespace Alma.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        public string Name { get; set; }
        public IndexType Type { get; set; }

        public IndexAttribute()
        {
            Name = string.Empty;
            Type = IndexType.Ascending;
        }

        public IndexAttribute(IndexType type)
        {
            Name = string.Empty;
            Type = type;
        }

        public IndexAttribute(string name, IndexType type = IndexType.Ascending)
        {
            Name = name;
            Type = type;
        }
    }

    public enum IndexType
    {
        Ascending,
        Descending
    }
}