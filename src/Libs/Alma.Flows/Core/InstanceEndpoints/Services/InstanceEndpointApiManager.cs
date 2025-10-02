using Alma.Flows.Core.InstanceEndpoints.Models;
using Microsoft.Extensions.Logging;

namespace Alma.Flows.Core.InstanceEndpoints.Services
{
    public interface IInstanceEndpointApiManager
    {
        public ValueTask<List<InstanceApiModel>> ListApis(string discriminator);
    }

    public class InstanceEndpointApiManager : IInstanceEndpointApiManager
    {
        private readonly ILogger<InstanceEndpointApiManager> _logger;

        public InstanceEndpointApiManager(ILogger<InstanceEndpointApiManager> logger)
        {
            _logger = logger;
            _logger.LogError("InstanceEndpointApiManager is not implemented yet. This is a placeholder class.");

            throw new NotImplementedException("InstanceEndpointApiManager is not implemented yet. This is a placeholder class.");
        }

        public ValueTask<List<InstanceApiModel>> ListApis(string discriminator)
        {
            throw new NotImplementedException();
        }
    }
}