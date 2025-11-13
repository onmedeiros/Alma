using System.Collections.Concurrent;
using System.Reflection;

namespace Alma.Workflows.Core.Properties
{
    /// <summary>
    /// Thread-safe cache for PropertyInfo instances to avoid expensive reflection operations.
    /// Implements singleton pattern for global cache access.
    /// </summary>
    public sealed class PropertyCache
    {
        private static readonly Lazy<PropertyCache> _instance = 
            new Lazy<PropertyCache>(() => new PropertyCache());

        private readonly ConcurrentDictionary<string, PropertyInfo[]> _typePropertiesCache = new();
        private readonly ConcurrentDictionary<string, PropertyInfo?> _propertyInfoCache = new();

        public static PropertyCache Instance => _instance.Value;

        private PropertyCache() { }

        /// <summary>
        /// Gets all properties of a type, filtered by a predicate.
        /// Results are cached per type and predicate combination.
        /// </summary>
        public PropertyInfo[] GetProperties(Type type, Func<PropertyInfo, bool> predicate)
        {
            var cacheKey = $"{type.FullName}_{predicate.Method.Name}_{predicate.GetHashCode()}";

            return _typePropertiesCache.GetOrAdd(cacheKey, _ =>
            {
                return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(predicate)
                    .ToArray();
            });
        }

        /// <summary>
        /// Gets a specific PropertyInfo by type and property name.
        /// Results are cached.
        /// </summary>
        public PropertyInfo? GetPropertyInfo(Type type, string propertyName)
        {
            var cacheKey = $"{type.FullName}.{propertyName}";

            return _propertyInfoCache.GetOrAdd(cacheKey, _ =>
            {
                return type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            });
        }

        /// <summary>
        /// Clears all cached property information.
        /// Useful for testing or when types are dynamically loaded/unloaded.
        /// </summary>
        public void Clear()
        {
            _typePropertiesCache.Clear();
            _propertyInfoCache.Clear();
        }

        /// <summary>
        /// Gets cache statistics for monitoring.
        /// </summary>
        public (int TypesCached, int PropertiesCached) GetStatistics()
        {
            return (_typePropertiesCache.Count, _propertyInfoCache.Count);
        }
    }
}
