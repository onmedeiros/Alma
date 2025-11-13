namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Concrete implementation of activity data state management.
    /// </summary>
    public class ActivityDataState : IActivityDataState
    {
        private readonly Dictionary<string, Dictionary<string, object?>> _activityData;

        public ActivityDataState()
        {
            _activityData = new Dictionary<string, Dictionary<string, object?>>();
        }

        public IReadOnlyDictionary<string, Dictionary<string, object?>> GetAll()
        {
            return _activityData;
        }

        public Dictionary<string, object?> GetActivityData(string activityId)
        {
            if (!_activityData.TryGetValue(activityId, out var data))
            {
                data = new Dictionary<string, object?>();
                _activityData.Add(activityId, data);
            }
            return data;
        }

        public void SetActivityData(string activityId, Dictionary<string, object?> data)
        {
            _activityData.Remove(activityId);
            _activityData.Add(activityId, data);
        }

        public object? GetDataValue(string activityId, string key)
        {
            var data = GetActivityData(activityId);
            return data.TryGetValue(key, out var value) ? value : null;
        }

        public void SetDataValue(string activityId, string key, object? value)
        {
            var data = GetActivityData(activityId);
            data[key] = value;
        }

        public bool HasActivityData(string activityId)
        {
            return _activityData.ContainsKey(activityId);
        }

        public bool RemoveActivityData(string activityId)
        {
            return _activityData.Remove(activityId);
        }

        public void Clear()
        {
            _activityData.Clear();
        }
    }
}
