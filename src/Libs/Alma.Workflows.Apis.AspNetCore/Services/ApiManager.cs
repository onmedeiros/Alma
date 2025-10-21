using Alma.Integrations.Apis.Services;
using Alma.Integrations.Apis.Validators;
using Alma.Workflows.Core.InstanceEndpoints.Models;
using Alma.Workflows.Core.InstanceEndpoints.Services;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Apis.AspNetCore.Services
{
    public class ApiManager : IInstanceEndpointApiManager
    {
        private readonly ILogger<ApiManager> _logger;
        private readonly IApiService _apiService;

        public ApiManager(ILogger<ApiManager> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        public async ValueTask<List<InstanceApiModel>> ListApis(string discriminator)
        {
            var apis = await _apiService.SearchAsync(new ApiSearchModel
            {
                PageIndex = 1,
                PageSize = int.MaxValue,
                OrganizationId = discriminator
            });

            return apis.Select(s => new InstanceApiModel
            {
                Id = s.Id,
                Name = s.Name,
                Path = s.Path
            })
                .OrderBy(x => x.Name)
                .ToList();
        }
    }
}