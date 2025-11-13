namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Manages activity data state during flow execution.
    /// </summary>
    public interface IActivityDataState
    {
        /// <summary>
        /// Gets all activity data.
        /// </summary>
        IReadOnlyDictionary<string, Dictionary<string, object?>> GetAll();

        /// <summary>
        /// Gets data for a specific activity.
        /// </summary>
        Dictionary<string, object?> GetActivityData(string activityId);

        /// <summary>
        /// Sets data for a specific activity.
        /// </summary>
        void SetActivityData(string activityId, Dictionary<string, object?> data);

        /// <summary>
        /// Gets a specific data value for an activity.
        /// </summary>
        object? GetDataValue(string activityId, string key);

        /// <summary>
        /// Sets a specific data value for an activity.
        /// </summary>
        void SetDataValue(string activityId, string key, object? value);

        /// <summary>
        /// Checks if an activity has data.
        /// </summary>
        bool HasActivityData(string activityId);

        /// <summary>
        /// Removes all data for a specific activity.
        /// </summary>
        bool RemoveActivityData(string activityId);

        /// <summary>
        /// Clears all activity data.
        /// </summary>
        void Clear();
    }
}
