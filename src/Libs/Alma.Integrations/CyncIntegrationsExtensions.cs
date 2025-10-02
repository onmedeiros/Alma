using Alma.Integrations.Apis.Services;
using Microsoft.Extensions.DependencyInjection;
using Alma.Core;

namespace Alma.Integrations
{
    public static class AlmaIntegrationsExtensions
    {
        public static IServiceCollection AddAlmaIntegrations(this IServiceCollection services)
        {
            services.AddScoped<IApiService, ApiService>();
            services.AddScoped<IApiKeyService, ApiKeyService>();
            services.AddSimpleValidator("Alma.Integrations");
            return services;
        }
    }
}