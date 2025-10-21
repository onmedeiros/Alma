namespace Alma.Workflows.Scripting
{
    public class ScriptExecutedPort
    {
        public required string Name { get; set; }
        public object? Data { get; set; }
    }
}