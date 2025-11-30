using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using System.Reflection;

namespace Alma.Workflows.Core.Properties
{
    /// <summary>
    /// Provides high-performance access to Port properties on activities.
    /// Uses caching to minimize reflection overhead.
    /// </summary>
    public class PortAccessor : IPropertyAccessor<Port>
    {
        private readonly PropertyCache _cache;

        public PortAccessor()
        {
            _cache = PropertyCache.Instance;
        }

        public Port? Get(IActivity activity, string name)
        {
            var propertyInfo = GetPropertyInfo(activity, name);
            if (propertyInfo == null)
                return null;

            return propertyInfo.GetValue(activity) as Port;
        }

        public void Set(IActivity activity, string name, object? value)
        {
            if (value is not Port port)
                throw new ArgumentException($"Value must be of type Port, but got {value?.GetType().Name ?? "null"}.", nameof(value));

            var propertyInfo = GetPropertyInfo(activity, name);
            if (propertyInfo == null)
                throw new InvalidOperationException($"Port property '{name}' not found on activity type '{activity.GetType().Name}'.");

            propertyInfo.SetValue(activity, port);
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
            var activityType = activity.GetType();
            var propertyInfo = _cache.GetPropertyInfo(activityType, name);

            if (propertyInfo != null && propertyInfo.PropertyType.IsAssignableTo(typeof(Port)))
                return propertyInfo;

            return null;
        }

        public IEnumerable<PropertyInfo> GetPropertyInfos(IActivity activity)
        {
            var activityType = activity.GetType();
            
            return _cache.GetProperties(activityType, p =>
                p.PropertyType.IsAssignableTo(typeof(Port)));
        }

        /// <summary>
        /// Gets all Port instances from an activity.
        /// </summary>
        public IEnumerable<Port> GetPorts(IActivity activity)
        {
            var portProperties = GetPropertyInfos(activity);

            foreach (var portProperty in portProperties)
            {
                var port = portProperty.GetValue(activity) as Port;
                if (port != null)
                    yield return port;
            }
        }

        /// <summary>
        /// Sets the data value of a port.
        /// </summary>
        public void SetPortData(IActivity activity, string name, object? value)
        {
            var port = Get(activity, name);
            if (port == null)
                throw new InvalidOperationException($"Port '{name}' not found on activity type '{activity.GetType().Name}'.");

            port.Data = value;
        }

        /// <summary>
        /// Gets the data value from a port.
        /// </summary>
        public object? GetPortData(IActivity activity, string name)
        {
            var port = Get(activity, name);
            return port?.Data;
        }
    }
}
