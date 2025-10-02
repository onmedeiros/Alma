using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SimpleCore.Scopes
{
    public interface IScopeManager
    {

    }

    public class ScopeManager : IScopeManager
    {
        private readonly ILogger<ScopeManager> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ScopeManager(ILogger<ScopeManager> logger)
        {
            _logger = logger;
        }
    }
}
