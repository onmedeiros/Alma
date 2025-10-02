using Alma.Integrations.Apis.Services;
using Alma.Modules.Integrations.Controllers.Filters;
using Alma.Organizations.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Alma.Modules.Integrations.Controllers
{
    [ApiController]
    [ServiceFilter(typeof(ApiHostRestrictionFilter))]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private readonly IOrganizationService _organizationService;
        private readonly IApiService _apiService;
        private readonly IApiInvoker _apiInvoker;

        public ApiController(ILogger<ApiController> logger, IOrganizationService organizationService, IApiService apiService, IApiInvoker apiInvoker)
        {
            _logger = logger;
            _organizationService = organizationService;
            _apiService = apiService;
            _apiInvoker = apiInvoker;
        }

        [HttpGet("{subdomain}/ping", Order = int.MaxValue - 1)]
        public async ValueTask<IActionResult> Ping([FromRoute] string subdomain)
        {
            var organization = await _organizationService.GetBySubdomain(subdomain);

            if (organization is null)
                return NotFound($"Organization with subdomain '{subdomain}' not found.");

            return Ok($"Hello {organization.Name}!");
        }

        [HttpGet("{subdomain:regex(^((?!_content|_framework).)*$)}/{path}/{*endpoint}", Order = int.MaxValue)]
        public async ValueTask<IActionResult> Get([FromRoute] string subdomain, [FromRoute] string path, [FromRoute] string endpoint)
        {
            var organization = await _organizationService.GetBySubdomain(subdomain);

            if (organization is null)
                return NotFound($"Organization with subdomain '{subdomain}' not found.");

            var api = await _apiService.GetByPathAsync(path, organization.Id);

            if (api is null)
                return NotFound($"API with base path '{path}' not found in organization '{subdomain}'.");

            var result = await _apiInvoker.InvokeGetAsync(api, endpoint);

            return StatusCode(result.StatusCode, result.Content);
        }

        [HttpPost("{subdomain:regex(^((?!_content|_framework).)*$)}/{path}/{*endpoint}", Order = int.MaxValue)]
        public async ValueTask<IActionResult> Post([FromRoute] string subdomain, [FromRoute] string path, [FromRoute] string endpoint)
        {
            var organization = await _organizationService.GetBySubdomain(subdomain);

            if (organization is null)
                return NotFound($"Organization with subdomain '{subdomain}' not found.");

            var api = await _apiService.GetByPathAsync(path, organization.Id);

            if (api is null)
                return NotFound($"API with base path '{path}' not found in organization '{subdomain}'.");

            var queryString = HttpContext.Request.QueryString.HasValue ? HttpContext.Request.QueryString.Value : null;
            var content = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var contentType = HttpContext.Request.ContentType;

            var result = await _apiInvoker.InvokePostAsync(api, endpoint, queryString, content, contentType);

            return StatusCode(result.StatusCode, result.Content);
        }
    }
}