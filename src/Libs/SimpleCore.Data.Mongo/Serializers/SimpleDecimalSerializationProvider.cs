using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

namespace SimpleCore.Data.Mongo.Serializers
{
    public class SimpleDecimalSerializationProvider : IBsonSerializationProvider
    {
        private static readonly DecimalSerializer DecimalSerializer = new DecimalSerializer(BsonType.Decimal128);
        private static readonly NullableSerializer<decimal> NullableSerializer = new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128));

        public IBsonSerializer? GetSerializer(Type type)
        {
            if (type == typeof(decimal)) return DecimalSerializer;
            if (type == typeof(decimal?)) return NullableSerializer;

            return null; // falls back to Mongo defaults
        }
    }
}
