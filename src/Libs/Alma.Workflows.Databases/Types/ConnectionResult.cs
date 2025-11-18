namespace Alma.Workflows.Databases.Types
{
    public class ConnectionResult
    {
        public bool Succeeded { get; init; }
        public string? Message { get; init; }
        public string? Details { get; init; }
    }
}