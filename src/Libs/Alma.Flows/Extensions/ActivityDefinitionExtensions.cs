using Alma.Flows.Definitions;
using Alma.Core.Extensions;
namespace Alma.Flows.Extensions
{
    public static class ActivityDefinitionExtensions
    {
        public static T? GetMetadata<T>(this ActivityDefinition activity, string key, T? @default = default(T))
        {
            if (!activity.Metadata.ContainsKey(key))
            {
                activity.Metadata.Add(key, @default?.ToString() ?? string.Empty);
                return @default;
            }

            var metadata = activity.Metadata[key];

            if (typeof(T) == typeof(int))
                return (T)(object)int.Parse(metadata.IsNullOrEmpty("0"));
            else if (typeof(T) == typeof(long))
                return (T)(object)long.Parse(metadata.IsNullOrEmpty("0"));
            else if (typeof(T) == typeof(float))
                return (T)(object)float.Parse(metadata.IsNullOrEmpty("0"));
            else if (typeof(T) == typeof(double))
                return (T)(object)double.Parse(metadata.IsNullOrEmpty("0"));
            else if (typeof(T) == typeof(decimal))
                return (T)(object)decimal.Parse(metadata.IsNullOrEmpty("0"));
            else if (typeof(T) == typeof(bool))
                return (T)(object)bool.Parse(metadata.IsNullOrEmpty("false"));
            else if (typeof(T) == typeof(string))
                return (T)(object)metadata;

            return @default;
        }

        public static void SetMetadata(this ActivityDefinition activity, string key, object value)
        {
            var exists = activity.Metadata.TryGetValue(key, out var _);

            if (exists)
                activity.Metadata[key] = value.ToString() ?? string.Empty;
            else
                activity.Metadata.Add(key, value.ToString() ?? string.Empty);
        }
    }
}
