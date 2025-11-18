namespace Alma.Workflows.Databases.Types
{
    public class QueryRequest
    {
        public required string Collection { get; set; }
        public string? Filter { get; set; }
    }
}