namespace Alma.Workflows.Core.States.Observers
{
    /// <summary>
    /// Observer that collects metrics on state changes.
    /// </summary>
    public class MetricsStateObserver : IStateObserver
    {
        private readonly Dictionary<string, int> _changeTypeCounts;
        private readonly object _lock = new();

        public MetricsStateObserver()
        {
            _changeTypeCounts = new Dictionary<string, int>();
        }

        public void OnStateChanged(StateChangeEvent changeEvent)
        {
            lock (_lock)
            {
                if (!_changeTypeCounts.ContainsKey(changeEvent.ChangeType))
                {
                    _changeTypeCounts[changeEvent.ChangeType] = 0;
                }
                _changeTypeCounts[changeEvent.ChangeType]++;
            }
        }

        /// <summary>
        /// Gets metrics for all change types.
        /// </summary>
        public IReadOnlyDictionary<string, int> GetMetrics()
        {
            lock (_lock)
            {
                return new Dictionary<string, int>(_changeTypeCounts);
            }
        }

        /// <summary>
        /// Gets the count for a specific change type.
        /// </summary>
        public int GetCount(string changeType)
        {
            lock (_lock)
            {
                return _changeTypeCounts.TryGetValue(changeType, out var count) ? count : 0;
            }
        }

        /// <summary>
        /// Resets all metrics.
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _changeTypeCounts.Clear();
            }
        }
    }
}
