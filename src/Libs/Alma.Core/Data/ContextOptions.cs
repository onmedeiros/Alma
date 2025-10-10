namespace Alma.Core.Data
{
    public class ContextOptions
    {
        public ICollection<EntityIndex> Indexes { get; set; } = [];

        public ContextOptions AddIndex<T>(EntityIndexType type, string[] properties, string? name = null)
        {
            if (properties is null || properties.Length == 0)
                throw new ArgumentException("At least one property must be provided", nameof(properties));

            var item = new EntityIndex
            {
                EntityType = typeof(T),
                Type = type,
                Properties = properties,
                Name = name
            };

            Indexes.Add(item);
            return this;
        }
    }
}