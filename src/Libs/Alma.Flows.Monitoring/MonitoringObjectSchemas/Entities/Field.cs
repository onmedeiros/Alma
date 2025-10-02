namespace Alma.Flows.Monitoring.MonitoringObjectSchemas.Entities
{
    public class Field
    {
        public required string Name { get; set; }
        public required FieldType Type { get; set; }
        public int Order { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsRequired { get; set; }
        public bool IsTimestamp { get; set; }

        public bool Equals(Field? other)
        {
            if (other is null) return false;

            return Name == other.Name
                && Type == other.Type
                && Order == other.Order
                && IsPrimaryKey == other.IsPrimaryKey
                && IsRequired == other.IsRequired
                && IsTimestamp == other.IsTimestamp;
        }

        public override bool Equals(object? obj) => Equals(obj as Field);

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Type, Order, IsPrimaryKey, IsRequired, IsTimestamp);
        }
    }
}