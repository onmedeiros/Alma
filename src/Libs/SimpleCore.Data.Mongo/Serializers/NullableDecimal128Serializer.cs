using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

namespace SimpleCore.Data.Mongo.Serializers
{
    public class NullableDecimal128Serializer : SerializerBase<decimal?>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, decimal? value)
        {
            if (value.HasValue)
                context.Writer.WriteDecimal128(new Decimal128(value.Value));
            else
                context.Writer.WriteNull();
        }

        public override decimal? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Null:
                    context.Reader.ReadNull();
                    return null;
                case BsonType.Decimal128:
                    return Decimal128.ToDecimal(context.Reader.ReadDecimal128());
                default:
                    throw new BsonSerializationException($"Cannot deserialize BsonType {bsonType} to decimal?");
            }
        }
    }
}
