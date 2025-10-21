namespace Alma.Workflows.States
{
    public class ExecutionHistory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public DateTime DateTime { get; set; } = DateTime.Now;
        public string? ActivityFullName { get; set; }
        public string? ActivityId { get; set; }

        public ICollection<PortExecutionHistory> ExecutedPorts { get; set; } = [];
    }
}
