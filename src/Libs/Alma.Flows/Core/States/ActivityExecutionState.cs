using Alma.Flows.Enums;

namespace Alma.Flows.States
{
    public class ActivityState
    {
        public required int Sequential { get; set; }
        public required string ActivityId { get; set; }
        public string? ActivityPort { get; set; }
        public object? Data { get; set; }
        public ActivityExecutionStatus ExecutionState { get; set; } = ActivityExecutionStatus.Pending;
    }
}
