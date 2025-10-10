using Alma.Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Organizations.Entities
{
    [Table("organizations.OrganizationUser")]
    public class OrganizationUser : Entity
    {
        public required string OrganizationId { get; set; }
        public required string UserId { get; set; }
        public bool IsCurrent { get; set; } = false;
    }
}