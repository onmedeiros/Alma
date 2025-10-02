using Alma.Core.Entities;

namespace Alma.Integrations.Apis.Entities
{
    public class Api : Entity
    {
        public required string OrganizationId { get; set; }
        public required string Name { get; set; }
        public required string Path { get; set; }
        public required bool IsActive { get; set; }
    }
}