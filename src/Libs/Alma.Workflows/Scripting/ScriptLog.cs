namespace Alma.Workflows.Scripting
{
    public class ScriptLog
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Message { get; set; } = string.Empty;
    }
}