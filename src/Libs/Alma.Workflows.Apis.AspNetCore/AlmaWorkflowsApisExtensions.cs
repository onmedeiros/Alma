using Alma.Integrations.Apis.Services;
using Alma.Workflows.Apis.AspNetCore.Services;
using Alma.Workflows.Core.InstanceEndpoints.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows.Apis.AspNetCore
{
    public static class AlmaWorkflowsApisExtensions
    {
        public static IServiceCollection AddAlmaWorkflowsApis(this IServiceCollection services)
        {
            // Register the API manager for ASP.NET Core
            services.AddScoped<IInstanceEndpointApiManager, ApiManager>();
            services.AddScoped<IApiInvoker, WorkflowsApiInvoker>();

            return services;
        }
    }
}