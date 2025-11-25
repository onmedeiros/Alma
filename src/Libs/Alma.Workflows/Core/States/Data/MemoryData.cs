using Alma.Workflows.Core.ApprovalsAndChecks.Models;

namespace Alma.Workflows.Core.States.Data
{
    public class MemoryData
    {
        public required string ActivityId { get; set; }
        public Dictionary<string, ValueObject> Data { get; set; } = [];
    }
}