using Alma.Core.Entities;

namespace Alma.Organizations.Entities
{
    public class Organization : Entity
    {
        public required string Subdomain { get; set; }
        public required string Name { get; set; }
    }
}