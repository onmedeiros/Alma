using Alma.Integrations.Apis.Entities;
using Alma.Integrations.Apis.Models;
using Microsoft.Extensions.Logging;

namespace Alma.Integrations.Apis.Services
{
    public interface IApiInvoker
    {
        public ValueTask<ApiInvokeResult> InvokeGetAsync(Api api, string endpoint, CancellationToken cancellationToken = default);

        ValueTask<ApiInvokeResult> InvokePostAsync(Api api, string endpoint, string? queryString = null, string? content = null, string? contentType = null, CancellationToken cancellationToken = default);
    }

    public class ApiInvoker : IApiInvoker
    {
        private readonly ILogger<ApiInvoker> _logger;

        public ApiInvoker(ILogger<ApiInvoker> logger)
        {
            _logger = logger;
        }

        public ValueTask<ApiInvokeResult> InvokeGetAsync(Api api, string endpoint, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<ApiInvokeResult> InvokePostAsync(Api api, string endpoint, string? queryString = null, string? content = null, string? contentType = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}