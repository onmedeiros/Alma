using Alma.Core.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alma.Workflows.Core.ApprovalsAndChecks.Models
{
    /// <summary>
    /// Represents a value object that can serialize and deserialize its value.
    /// Value objects are used to store any value that needs to be persisted in the workflow state.
    /// </summary>
    public class ValueObject
    {
        private bool _desserialized = false;
        private object? _value;

        /// <summary>
        /// Gets the runtime type of the value stored in the ValueObject.
        /// </summary>
        public string Type { get; init; } = default!;

        /// <summary>
        /// Gets the value stored in the ValueObject.
        /// </summary>
        public object? Value => GetValue();

        /// <summary>
        /// Gets the string representation of the value, or <see langword="null"/> if no value is set.
        /// </summary>
        public string? ValueString { get; init; }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(), new ObjectToInferredTypesConverter() }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueObject"/> class with the specified value.
        /// </summary>
        /// <param name="value">Value to be stored.</param>
        public ValueObject(object? value)
        {
            // For non-primitive, non-string types, store the assembly-qualified name so we can reliably resolve the type later
            if (value is null)
            {
                Type = "null";
                ValueString = "null";
                _desserialized = true;
                return;
            }

            var valueType = value.GetType();
            if (valueType.IsPrimitive || value is string || value is decimal || value is DateTime)
            {
                // Keep previous behavior/compat for primitives and common BCL scalars
                Type = valueType.FullName ?? valueType.Name;
                ValueString = value.ToString();
            }
            else
            {
                // Use assembly-qualified name so Type resolution works even if the assembly is not yet loaded
                Type = valueType.AssemblyQualifiedName ?? (valueType.FullName ?? valueType.Name);
                ValueString = JsonSerializer.Serialize(value);
            }

            _value = value;
            _desserialized = true;
        }

        /// <summary>
        /// Gets the value stored in the ValueObject.
        /// </summary>
        /// <returns>Returns value.</returns>
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
                case "System.String":
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

        /// <summary>
        /// Deserializes the stored JSON string to an object of the specified type.
        /// </summary>
        /// <returns>An object representing the deserialized value, or null if the stored value is "null" or empty.</returns>
        /// <exception cref="Exception">Thrown if the specified type cannot be found.</exception>
        public object? GetDesserializedValue()
        {
            if (ValueString == "null" || string.IsNullOrEmpty(ValueString))
                return null;

            // Try direct resolution first (works for assembly-qualified names and mscorlib types)
            var type = System.Type.GetType(Type);

            // Fallback: search already loaded assemblies by FullName
            if (type is null)
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(Type, throwOnError: false, ignoreCase: false);
                    if (type is not null)
                        break;
                }
            }

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