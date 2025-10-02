namespace SimpleCore.Data.Mongo.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        public IndexType Type { get; set; }

        public IndexAttribute(IndexType type)
        {
            Type = type;
        }
    }



    public enum IndexType
    {
        Ascending,
        Descending
    }
}
