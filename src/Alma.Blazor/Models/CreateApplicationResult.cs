using Alma.Blazor.Entities;

namespace Alma.Blazor.Models
{
    public class CreateApplicationResult
    {
        public required string ClientSecret { get; set; }
        public required AlmaApplication Application { get; set; }
    }
}
