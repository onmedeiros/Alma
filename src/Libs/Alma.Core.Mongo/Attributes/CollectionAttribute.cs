namespace SimpleCore.Data.Mongo.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectionAttribute : Attribute
    {
        public string Name { get; }

        public CollectionAttribute(string name)
        {
            Name = name;
        }
    }
}
