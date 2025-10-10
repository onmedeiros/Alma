using Alma.Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Integrations.Apis.Entities
{
    [Table("integrations.Api")]
    public class Api : Entity
    {
        public required string OrganizationId { get; set; }
        public required string Name { get; set; }
        public required string Path { get; set; }
        public required bool IsActive { get; set; }
    }
}