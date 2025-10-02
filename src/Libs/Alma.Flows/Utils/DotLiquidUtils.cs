using DotLiquid;
using System.Text.Json;

namespace Alma.Flows.Utils
{
    public static class DotLiquidUtils
    {
        public static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public static Hash ConvertJsonToHash(string json)
        {
            JsonDocument doc = JsonDocument.Parse(json);
            var data = (Dictionary<string, object>)ConvertJsonElement(doc.RootElement);

            return Hash.FromDictionary(data);
        }

        public static object ConvertJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object>();
                    foreach (var property in element.EnumerateObject())
                    {
                        dict[property.Name] = ConvertJsonElement(property.Value);
                    }
                    return dict;

                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        list.Add(ConvertJsonElement(item));
                    }
                    return list;

                case JsonValueKind.String:
                    return element.GetString();

                case JsonValueKind.Number:
                    return element.GetDecimal(); // Ou GetInt32(), dependendo do caso
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
    }
}