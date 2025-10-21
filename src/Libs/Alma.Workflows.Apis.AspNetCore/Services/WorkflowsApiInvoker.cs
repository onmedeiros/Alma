using Alma.Integrations.Apis.Entities;
using Alma.Integrations.Apis.Models;
using Alma.Integrations.Apis.Services;
using Alma.Workflows.Core.Common.Enums;
using Alma.Workflows.Core.InstanceEndpoints.Services;
using Alma.Workflows.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using Alma.Workflows.Core.InstanceExecutions.Services;
using System.Net.Mime;
using Alma.Core.Utils;

namespace Alma.Workflows.Apis.AspNetCore.Services
{
    public class WorkflowsApiInvoker : IApiInvoker
    {
        private readonly ILogger<WorkflowsApiInvoker> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IInstanceEndpointManager _instanceEndpointManager;
        private readonly IFlowRunManager _flowRunManager;
        private readonly IInstanceExecutionRunner _instanceExecutionRunner;

        public WorkflowsApiInvoker(ILogger<WorkflowsApiInvoker> logger, IHttpContextAccessor httpContextAccessor, IInstanceEndpointManager instanceEndpointManager, IFlowRunManager flowRunManager, IInstanceExecutionRunner instanceExecutionRunner)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _instanceEndpointManager = instanceEndpointManager;
            _flowRunManager = flowRunManager;
            _instanceExecutionRunner = instanceExecutionRunner;
        }

        public async ValueTask<ApiInvokeResult> InvokeGetAsync(Api api, string endpoint, CancellationToken cancellationToken)
        {
            // Find the instance endpoint for the given API and endpoint path
            var instanceEndpoint = await _instanceEndpointManager.FindByPath(api.Id, endpoint, ApiMethod.Get, api.OrganizationId, cancellationToken);

            if (instanceEndpoint == null)
            {
                _logger.LogWarning("No instance found for API {ApiId} and endpoint {Endpoint}", api.Id, endpoint);
                return new ApiInvokeResult { StatusCode = (int)HttpStatusCode.NotFound };
            }

            // Build parameters from query string
            var parameters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            // TODO: remove this dependency on HttpContext, transfer to Controller
            var httpContext = _httpContextAccessor.HttpContext;
            var query = httpContext?.Request?.Query;

            if (query is not null && query.Count > 0)
            {
                foreach (var pair in query)
                {
                    var values = pair.Value;
                    object? value = values.Count switch
                    {
                        0 => null,
                        1 => values[0],
                        _ => values.ToArray()
                    };

                    parameters[pair.Key] = value;
                }
            }

            // Run the associated flow instance with query parameters
            var options = new ExecutionOptions
            {
                Parameters = parameters
            };

            var executionContext = await _flowRunManager.RunAsync(instanceEndpoint.InstanceId, instanceEndpoint.Discriminator, options);

            var hasResponse = executionContext.State.Variables.TryGetValue("HttpResponse", out var httpResponse);

            if (!hasResponse)
                return new ApiInvokeResult { StatusCode = (int)HttpStatusCode.OK };

            if (httpResponse is not null && httpResponse.Value is Models.Activities.HttpResponse)
            {
                var response = (Models.Activities.HttpResponse)httpResponse.Value;

                return new ApiInvokeResult
                {
                    StatusCode = (int)response.StatusCode,
                    Content = response.Content
                };
            }
            else
            {
                _logger.LogWarning("Flow execution did not return a valid HttpResponse object for instance {InstanceId}", instanceEndpoint.InstanceId);
                return new ApiInvokeResult { StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        }

        public async ValueTask<ApiInvokeResult> InvokePostAsync(Api api, string endpoint, string? queryString = null, string? content = null, string? contentType = null, CancellationToken cancellationToken = default)
        {
            // Find the instance endpoint for the given API and endpoint path
            var instanceEndpoint = await _instanceEndpointManager.FindByPath(api.Id, endpoint, ApiMethod.Post, api.OrganizationId, cancellationToken);

            if (instanceEndpoint == null)
            {
                _logger.LogWarning("No instance found for API {ApiId} and endpoint {Endpoint}", api.Id, endpoint);
                return new ApiInvokeResult { StatusCode = (int)HttpStatusCode.NotFound };
            }

            // Build parameters
            var parameters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(queryString))
            {
                foreach (var pair in ParseQueryString(queryString))
                {
                    parameters[pair.Key] = pair.Value;
                }
            }

            if (!string.IsNullOrEmpty(content))
            {
                if (contentType == "application/json")
                {
                    var contentObject = JsonUtils.ConvertToDictionary(content);

                    foreach (var pair in contentObject ?? [])
                    {
                        parameters[pair.Key] = pair.Value;
                    }
                }
                else
                {
                    _logger.LogWarning("Unsupported content type {ContentType} for API {ApiId} and endpoint {Endpoint}", contentType, api.Id, endpoint);
                    return new ApiInvokeResult { StatusCode = (int)HttpStatusCode.UnsupportedMediaType };
                }
            }

            // Run the associated flow instance with query parameters
            var options = new ExecutionOptions
            {
                Parameters = parameters
            };

            var executionContext = await _instanceExecutionRunner.ExecuteAsync(instanceEndpoint.InstanceId, instanceEndpoint.Discriminator, options);

            var hasResponse = executionContext.State.Variables.TryGetValue("HttpResponse", out var httpResponse);

            if (!hasResponse)
                return new ApiInvokeResult { StatusCode = (int)HttpStatusCode.OK };

            if (httpResponse is not null && httpResponse.Value is Models.Activities.HttpResponse)
            {
                var response = (Models.Activities.HttpResponse)httpResponse.Value;

                return new ApiInvokeResult
                {
                    StatusCode = (int)response.StatusCode,
                    Content = response.Content
                };
            }
            else
            {
                _logger.LogWarning("Flow execution did not return a valid HttpResponse object for instance {InstanceId}", instanceEndpoint.InstanceId);
                return new ApiInvokeResult { StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        }

        private ICollection<KeyValuePair<string, object?>> ParseQueryCollection(IQueryCollection collection)
        {
            var parameters = new List<KeyValuePair<string, object?>>();
            foreach (var pair in collection)
            {
                var values = pair.Value;
                object? value = values.Count switch
                {
                    0 => null,
                    1 => values[0],
                    _ => values.ToArray()
                };
                parameters.Add(new KeyValuePair<string, object?>(pair.Key, value));
            }
            return parameters;
        }

        private ICollection<KeyValuePair<string, object?>> ParseQueryString(string queryString)
        {
            var parameters = new List<KeyValuePair<string, object?>>();

            if (string.IsNullOrEmpty(queryString))
                return parameters;

            var query = queryString.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in query)
            {
                var kvp = pair.Split('=', 2);
                var key = WebUtility.UrlDecode(kvp[0]);
                var value = kvp.Length > 1 ? WebUtility.UrlDecode(kvp[1]) : null;
                parameters.Add(new KeyValuePair<string, object?>(key, value));
            }

            return parameters;
        }
    }
}