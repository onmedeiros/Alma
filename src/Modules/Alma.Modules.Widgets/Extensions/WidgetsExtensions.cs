using Alma.Modules.Widgets.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Modules.Widgets.Extensions
{
    public static class WidgetsExtensions
    {
        public static IServiceCollection ConfigureWidgets(this IServiceCollection services, Action<WidgetRegistryOptions> configureOptions)
        {
            services.Configure(configureOptions);
            return services;
        }
    }
}