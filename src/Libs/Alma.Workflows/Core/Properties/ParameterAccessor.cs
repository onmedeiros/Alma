using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Factories;
using System.Reflection;

namespace Alma.Workflows.Core.Properties
{
    /// <summary>
    /// Provides high-performance access to Parameter properties on activities.
    /// Uses caching to minimize reflection overhead.
    /// </summary>
    public class ParameterAccessor : IPropertyAccessor<object>
    {
        private readonly PropertyCache _cache;

        public ParameterAccessor()
        {
            _cache = PropertyCache.Instance;
        }

        public object? Get(IActivity activity, string name)
        {
            var propertyInfo = GetPropertyInfo(activity, name);
            if (propertyInfo == null)
                return null;

            return propertyInfo.GetValue(activity);
        }

        public void Set(IActivity activity, string name, object? value)
        {
            var propertyInfo = GetPropertyInfo(activity, name);
            if (propertyInfo == null)
                throw new InvalidOperationException($"Parameter property '{name}' not found on activity type '{activity.GetType().Name}'.");

            var parameterInstance = propertyInfo.GetValue(activity);
            var valueStringPropertyInfo = propertyInfo.PropertyType.GetProperty(nameof(Parameter<object>.ValueString));

            if (valueStringPropertyInfo == null)
                throw new InvalidOperationException($"Property 'ValueString' not found on parameter type '{propertyInfo.PropertyType.Name}'.");

            // Se ainda não existe instância do parâmetro, criar uma
            if (parameterInstance == null)
            {
                var parameterGenericType = propertyInfo.PropertyType.GenericTypeArguments[0];
                parameterInstance = ParameterFactory.CreateParameter(parameterGenericType, value);
                propertyInfo.SetValue(activity, parameterInstance);
                return;
            }

            // Atualizar valor existente
            if (value == null)
            {
                valueStringPropertyInfo.SetValue(parameterInstance, null);
                return;
            }

            valueStringPropertyInfo.SetValue(parameterInstance, value.ToString());
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
            var allParameters = GetPropertyInfos(activity);
            return allParameters.FirstOrDefault(p => p.Name == name);
        }

        public IEnumerable<PropertyInfo> GetPropertyInfos(IActivity activity)
        {
            var activityType = activity.GetType();
            
            return _cache.GetProperties(activityType, p =>
                p.PropertyType.IsGenericType &&
                p.PropertyType.GetGenericTypeDefinition() == typeof(Parameter<>));
        }

        /// <summary>
        /// Gets the value of a parameter as a string.
        /// </summary>
        public string? GetValueAsString(IActivity activity, string name)
        {
            var propertyInfo = GetPropertyInfo(activity, name);
            if (propertyInfo == null)
                return null;

            var parameter = propertyInfo.GetValue(activity);
            if (parameter == null)
                return null;

            var valueProperty = parameter.GetType().GetProperty("Value");
            if (valueProperty == null)
                return null;

            var value = valueProperty.GetValue(parameter);
            return value?.ToString();
        }

        /// <summary>
        /// Gets the typed value of a parameter.
        /// </summary>
        public TValue? GetValue<TValue>(IActivity activity, string name)
        {
            var propertyInfo = GetPropertyInfo(activity, name);
            if (propertyInfo == null)
                return default;

            var parameter = propertyInfo.GetValue(activity);
            if (parameter == null)
                return default;
                
            var valueProperty = parameter.GetType().GetProperty("Value");
            if (valueProperty == null)
                return default;
                
            var value = valueProperty.GetValue(parameter);
            return value is TValue typedValue ? typedValue : default;
        }
    }
}
