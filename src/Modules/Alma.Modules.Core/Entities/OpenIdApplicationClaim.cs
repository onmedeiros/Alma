using Alma.Core.Entities;
using SimpleCore.Data.Mongo.Attributes;

namespace Alma.Blazor.Entities
{
    [Collection("openiddict.applications.claims")]
    public class OpenIdApplicationClaim : Entity
    {
        public required string ClientId { get; set; }
        public required string Type { get; set; }
        public required string Value { get; set; }
    }
}