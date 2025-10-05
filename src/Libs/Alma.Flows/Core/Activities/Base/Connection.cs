namespace Alma.Flows.Core.Activities.Base
{
    public class Connection
    {
        public string Id { get; set; } = string.Empty;
        public Endpoint Source { get; set; } = default!;
        public Endpoint Target { get; set; } = default!;
    }
}