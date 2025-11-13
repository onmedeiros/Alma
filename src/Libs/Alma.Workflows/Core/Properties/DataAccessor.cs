using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Factories;
using System.Reflection;

namespace Alma.Workflows.Core.Properties
{
    /// <summary>
    /// Provides high-performance access to Data properties on activities.
    /// Uses caching to minimize reflection overhead.
    /// </summary>
    public class DataAccessor : IPropertyAccessor<object>
    {
        private readonly PropertyCache _cache;

        public DataAccessor()
        {
            _cache = PropertyCache.Instance;
        }

        public object? Get(IActivity activity, string name)
        {
            var propertyInfo = GetPropertyInfo(activity, name);
            if (propertyInfo == null)
                return null;

            var dataInstance = propertyInfo.GetValue(activity);
            if (dataInstance == null)
                return null;

            var valuePropertyInfo = dataInstance.GetType().GetProperty("Value");
            if (valuePropertyInfo == null)
                return null;

            return valuePropertyInfo.GetValue(dataInstance);
        }

        public void Set(IActivity activity, string name, object? value)
        {
            var propertyInfo = GetPropertyInfo(activity, name);
            if (propertyInfo == null)
                throw new InvalidOperationException($"Data property '{name}' not found on activity type '{activity.GetType().Name}'.");

            var valuePropertyInfo = propertyInfo.PropertyType.GetProperty("Value");
            if (valuePropertyInfo == null)
                throw new InvalidOperationException($"Property 'Value' not found on data type '{propertyInfo.PropertyType.Name}'.");

            var dataInstance = DataFactory.CreateData(valuePropertyInfo.PropertyType);
            valuePropertyInfo.SetValue(dataInstance, value);
            propertyInfo.SetValue(activity, dataInstance);
        }

        public bool Has(IActivity activity, string name)
        {
            return GetPropertyInfo(activity, name) != null;
        }

        public IEnumerable<string> GetNames(IActivity activity)
        {
            return GetPropertyInfos(activity).Select(p => p.Name);
        }

        public PropertyInfo? GetPropertyInfo(IActivity activity, string name)
        {
            var allDataProperties = GetPropertyInfos(activity);
            return allDataProperties.FirstOrDefault(p => p.Name == name);
        }

        public IEnumerable<PropertyInfo> GetPropertyInfos(IActivity activity)
        {
            var activityType = activity.GetType();
            
            return _cache.GetProperties(activityType, p =>
                p.PropertyType.IsGenericType &&
                p.PropertyType.GetGenericTypeDefinition() == typeof(Data<>));
        }

        /// <summary>
        /// Gets the typed value of a data property.
        /// </summary>
        public TValue? GetValue<TValue>(IActivity activity, string name)
        {
            var propertyInfo = GetPropertyInfo(activity, name);
            if (propertyInfo == null)
                return default;

            var dataInstance = propertyInfo.GetValue(activity);
            if (dataInstance == null)
                return default;

            var valueProperty = dataInstance.GetType().GetProperty("Value");
            if (valueProperty == null)
                return default;

            var value = valueProperty.GetValue(dataInstance);
            return value is TValue typedValue ? typedValue : default;
        }
    }
}
