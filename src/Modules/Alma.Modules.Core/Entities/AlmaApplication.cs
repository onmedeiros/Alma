using OpenIddict.MongoDb.Models;

namespace Alma.Blazor.Entities
{
    public class AlmaApplication : OpenIddictMongoDbApplication
    {
        public string? OrganizationId { get; set; }
        public string? Name { get; set; }
    }
}