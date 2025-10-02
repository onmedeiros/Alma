using Alma.Integrations.Apis.Services;
using Alma.Flows.Apis.AspNetCore.Services;
using Alma.Flows.Core.InstanceEndpoints.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Flows.Apis.AspNetCore
{
    public static class AlmaFlowsApisExtensions
    {
        public static IServiceCollection AddAlmaFlowsApis(this IServiceCollection services)
        {
            // Register the API manager for ASP.NET Core
            services.AddScoped<IInstanceEndpointApiManager, ApiManager>();
            services.AddScoped<IApiInvoker, FlowsApiInvoker>();

            return services;
        }
    }
}