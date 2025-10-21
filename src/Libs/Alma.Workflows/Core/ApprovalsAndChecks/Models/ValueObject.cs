using Alma.Core.Json;
using System.Text.Json;

namespace Alma.Workflows.Core.ApprovalsAndChecks.Models
{
    public class ValueObject
    {
        private bool _desserialized = false;
        private object? _value;

        public string Type { get; init; } = default!;
        public object? Value => GetValue();
        public string? ValueString { get; init; }

        private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(), new ObjectToInferredTypesConverter() }
        };

        public ValueObject(object? value)
        {
            Type = value?.GetType().FullName ?? "null";

            _value = value;
            _desserialized = true;

            if (value != null)
            {
                if (value.GetType().IsPrimitive || value is string)
                {
                    ValueString = value.ToString();
                }
                else
                {
                    ValueString = JsonSerializer.Serialize(value);
                }
            }
            else
            {
                ValueString = "null";
            }
        }

        public object? GetValue()
        {
            if (_desserialized)
                return _value;

            if (ValueString is null)
                return null;

            _desserialized = true;

            switch (Type)
            {
                case "null":
                    return null;

                case "Boolean":
                    _value = bool.Parse(ValueString);
                    return _value;

                case "Byte":
                    _value = byte.Parse(ValueString);
                    return _value;

                case "Int32":
                    _value = int.Parse(ValueString);
                    return _value;

                case "Decimal":
                    _value = decimal.Parse(ValueString);
                    return _value;

                case "String":
                    _value = ValueString;
                    return ValueString;

                case "DateTime":
                    _value = DateTime.Parse(ValueString);
                    return _value;

                default:
                    _value = GetDesserializedValue();
                    return _value;
            }
        }

        public object? GetDesserializedValue()
        {
            if (ValueString == "null" || string.IsNullOrEmpty(ValueString))
                return null;

            Type? type = System.Type.GetType(Type);

            if (type is null)
                throw new Exception($"Type {Type} not found.");

            return JsonSerializer.Deserialize(ValueString, type, _jsonOptions);
        }

        public override string ToString()
        {
            return ValueString ?? "null";
        }
    }
}