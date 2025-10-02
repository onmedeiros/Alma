using System.ComponentModel.DataAnnotations;

namespace Alma.Organizations.Models
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public class OrganizationCreateModel
    {
        [Required]
        public string Subdomain { get; set; }

        [Required]
        public string Name { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}
