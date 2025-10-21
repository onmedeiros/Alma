using Alma.Workflows.Core.Common.Enums;

namespace Alma.Workflows.Core.InstanceEndpoints.Models
{
    public class InstanceEndpointCreateModel
    {
        public required string InstanceId { get; set; }
        public string? Discriminator { get; set; }
        public required string Name { get; set; }
        public string? ApiId { get; set; }
        public required string Path { get; set; }
        public required ApiMethod Method { get; set; }
        public bool IsActive { get; set; }
    }
}