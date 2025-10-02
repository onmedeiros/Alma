using Alma.Core.Entities;

namespace Alma.Organizations.Entities
{
    public class OrganizationUser : Entity
    {
        public required string OrganizationId { get; set; }
        public required string UserId { get; set; }
        public bool IsCurrent { get; set; } = false;
    }
}