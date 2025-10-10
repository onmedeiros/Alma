using Alma.Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Organizations.Entities
{
    [Table("organizations.Organization")]
    public class Organization : Entity
    {
        public required string Subdomain { get; set; }
        public required string Name { get; set; }
    }
}