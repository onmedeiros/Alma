using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Concrete implementation of history state management.
    /// </summary>
    public class HistoryState : IHistoryState
    {
        private readonly List<ExecutionHistory> _history;

        public HistoryState()
        {
            _history = new List<ExecutionHistory>();
        }

        public int Count => _history.Count;

        public IReadOnlyCollection<ExecutionHistory> GetAll()
        {
            return _history.AsReadOnly();
        }

        public void Add(ExecutionHistory entry)
        {
            _history.Add(entry);
        }

        public IEnumerable<ExecutionHistory> GetByActivityId(string activityId)
        {
            return _history.Where(x => x.ActivityId == activityId);
        }

        public ExecutionHistory? GetLatest()
        {
            return _history.LastOrDefault();
        }

        public void Clear()
        {
            _history.Clear();
        }
    }
}
