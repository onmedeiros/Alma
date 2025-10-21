using Alma.Workflows.Core.Common.Enums;

namespace Alma.Workflows.Core.InstanceEndpoints.Stores
{
    public class InstanceEndpointFilters
    {
        public string? Discriminator { get; set; }
        public string? InstanceId { get; set; }
        public string? Name { get; set; }
        public ApiMethod? Method { get; set; }
    }
}