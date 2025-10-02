namespace Alma.Integrations.Apis.Models
{
    public class ApiEditModel
    {
        public required string Id { get; set; }
        public required string OrganizationId { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public bool? IsActive { get; set; }
    }
}