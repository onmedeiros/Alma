using Microsoft.Extensions.DependencyInjection;

namespace Alma.Core.Data
{
    public static class DataExtensions
    {
        public static IServiceCollection ConfigureContext(this IServiceCollection services, Action<ContextOptions> configure)
        {
            services.Configure(configure);
            return services;
        }
    }
}