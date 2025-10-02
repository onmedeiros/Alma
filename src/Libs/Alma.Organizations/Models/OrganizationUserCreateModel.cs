namespace Alma.Organizations.Models
{
    public class OrganizationUserCreateModel
    {
        public required string OrganizationId { get; set; }
        public required string UserId { get; set; }
        public bool? IsCurrent { get; set; }
    }
}