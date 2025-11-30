using Alma.Workflows.Core.Activities.Abstractions;
using System.Reflection;

namespace Alma.Workflows.Core.Properties
{
    /// <summary>
    /// Defines a contract for accessing and manipulating properties of activities.
    /// Provides a high-performance alternative to direct reflection usage.
    /// </summary>
    /// <typeparam name="TProperty">The type of property being accessed.</typeparam>
    public interface IPropertyAccessor<TProperty>
    {
        /// <summary>
        /// Gets a property value from an activity by name.
        /// </summary>
        /// <param name="activity">The activity containing the property.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>The property instance, or null if not found.</returns>
        TProperty? Get(IActivity activity, string name);

        /// <summary>
        /// Sets a property value on an activity by name.
        /// </summary>
        /// <param name="activity">The activity containing the property.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value to set.</param>
        void Set(IActivity activity, string name, object? value);

        /// <summary>
        /// Checks if an activity has a property with the specified name.
        /// </summary>
        /// <param name="activity">The activity to check.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>True if the property exists, otherwise false.</returns>
        bool Has(IActivity activity, string name);

        /// <summary>
        /// Gets all property names of the specified type from an activity.
        /// </summary>
        /// <param name="activity">The activity to inspect.</param>
        /// <returns>Collection of property names.</returns>
        IEnumerable<string> GetNames(IActivity activity);

        /// <summary>
        /// Gets the PropertyInfo for a property by name.
        /// Uses caching for performance.
        /// </summary>
        /// <param name="activity">The activity containing the property.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>The PropertyInfo, or null if not found.</returns>
        PropertyInfo? GetPropertyInfo(IActivity activity, string name);

        /// <summary>
        /// Gets all PropertyInfo instances for properties of the specified type.
        /// Uses caching for performance.
        /// </summary>
        /// <param name="activity">The activity to inspect.</param>
        /// <returns>Collection of PropertyInfo instances.</returns>
        IEnumerable<PropertyInfo> GetPropertyInfos(IActivity activity);
    }
}
