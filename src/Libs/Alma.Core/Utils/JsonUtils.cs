using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Alma.Core.Utils
{
    public static class JsonUtils
    {
        public static Dictionary<string, object?>? ConvertToDictionary(string json)
        {
            JsonDocument doc = JsonDocument.Parse(json);
            var data = (Dictionary<string, object?>?)ConvertJsonElement(doc.RootElement);

            return data;
        }

        public static object? ConvertJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object?>();
                    foreach (var property in element.EnumerateObject())
                    {
                        dict[property.Name] = ConvertJsonElement(property.Value);
                    }
                    return dict;

                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        var dictItem = ConvertJsonElement(item);
                        if (dictItem is not null)
                            list.Add(dictItem);
                    }
                    return list;

                case JsonValueKind.String:
                    var stringValue = element.GetString() ?? string.Empty;

                    if (IsValidDatetime(stringValue))
                        return DateTime.Parse(stringValue);
                    else
                        return stringValue;

                case JsonValueKind.Number:
                    return element.GetDouble(); // Ou GetInt32(), dependendo do caso
                case JsonValueKind.True:
                    return true;

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.Null:
                    return null;

                default:
                    throw new InvalidOperationException($"Unsupported JsonValueKind: {element.ValueKind}");
            }
        }

        public static bool IsValidDatetime(string value)
        {
            return DateTime.TryParse(value, out _);
        }
    }
}