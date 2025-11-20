namespace Alma.Workflows.Databases.Types
{
    public class CommandRequest
    {
        public required string Collection { get; set; }
        public string? Filter { get; set; }
    }
}