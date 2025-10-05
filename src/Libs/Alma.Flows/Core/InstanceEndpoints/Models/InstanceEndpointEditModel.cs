using Alma.Flows.Core.Common.Enums;

namespace Alma.Flows.Core.InstanceEndpoints.Models
{
    public class InstanceEndpointEditModel
    {
        public required string Id { get; set; }
        public required string InstanceId { get; set; }
        public string? Discriminator { get; set; }
        public string? ApiId { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public ApiMethod Method { get; set; }
        public bool? IsActive { get; set; }
    }
}