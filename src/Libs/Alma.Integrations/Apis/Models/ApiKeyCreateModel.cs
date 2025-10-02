namespace Alma.Integrations.Apis.Models
{
    public class ApiKeyCreateModel
    {
        public required string OrganizationId { get; set; }
        public required string ApiId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
