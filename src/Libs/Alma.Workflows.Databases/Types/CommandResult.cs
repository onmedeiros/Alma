namespace Alma.Workflows.Databases.Types
{
    public class CommandResult<T>
    {
        public bool Succeeded { get; init; }
        public T? Data { get; init; }
        public string? Message { get; init; }
        public string? Details { get; init; }
    }
}