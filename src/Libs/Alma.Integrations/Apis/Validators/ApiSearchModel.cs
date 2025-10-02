using Alma.Core.Types;

namespace Alma.Integrations.Apis.Validators
{
    public class ApiSearchModel : SearchModelBase
    {
        public required string OrganizationId { get; set; }
    }
}