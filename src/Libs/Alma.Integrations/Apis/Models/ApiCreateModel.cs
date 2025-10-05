namespace Alma.Integrations.Apis.Models
{
    public class ApiCreateModel
    {
        public required string OrganizationId { get; set; }
        public required string Name { get; set; }
        public required string Path { get; set; }
        public required bool IsActive { get; set; }
    }
}