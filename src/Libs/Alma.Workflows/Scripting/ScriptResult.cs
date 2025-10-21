namespace Alma.Workflows.Scripting
{
    public class ScriptResult
    {
        public ScriptResultStatus Status { get; private set; }
        public string? StatusDetails { get; set; }

        public ICollection<ScriptLog> Logs { get; } = [];
        public ICollection<ScriptExecutedPort> ExecutedPorts { get; } = [];

        public void Log(string message)
        {
            Logs.Add(new ScriptLog { Message = message });
        }

        public void ExecutePort(string name, object? data)
        {
            ExecutedPorts.Add(new ScriptExecutedPort
            {
                Name = name,
                Data = data
            });
        }

        public void Fail(string details)
        {
            Status = ScriptResultStatus.Failure;
            StatusDetails = details;
        }
    }
}