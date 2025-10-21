using Microsoft.Extensions.DependencyInjection;

namespace Alma.Modules.Workflows.Components.Registries
{
    public static class ActivityComponentRegistryExtensions
    {
        public static IServiceCollection AddActivityComponentRegistry(this IServiceCollection services, Action<ActivityComponentRegistryOptions>? options)
        {
            services.AddSingleton<IActivityComponentRegistry, ActivityComponentRegistry>();

            if (options != null)
                services.Configure(options);

            return services;
        }
    }
}
