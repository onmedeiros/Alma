namespace Alma.Organizations.Models
{
    public class OrganizationEditModel
    {
        public required string Id { get; set; }
        public string? Subdomain { get; set; }
        public string? Name { get; set; }
    }
}