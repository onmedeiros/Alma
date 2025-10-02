using Microsoft.Extensions.DependencyInjection;

namespace Alma.Flows.Design.Registries
{
    public static class ParameterEditorRegistryExtensions
    {
        public static IServiceCollection AddParameterEditorRegistry(this IServiceCollection services, Action<ParameterEditorRegistryOptions>? options = null)
        {
            services.AddSingleton<IParameterEditorRegistry, ParameterEditorRegistry>();

            if (options != null)
                services.Configure(options);

            return services;
        }

        public static IServiceCollection ConfigureParameterEditors(this IServiceCollection services, Action<ParameterEditorRegistryOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            services.Configure(options);

            return services;
        }
    }
}