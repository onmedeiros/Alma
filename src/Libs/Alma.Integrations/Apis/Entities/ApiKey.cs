using Alma.Core.Entities;

namespace Alma.Integrations.Apis.Entities
{
    public class ApiKey : Entity
    {
        public required string OrganizationId { get; set; }
        public required string ApiId { get; set; }
        public required string Name { get; set; }
        public required string Key { get; set; }
        public required bool IsActive { get; set; }
    }
}
