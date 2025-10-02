using Alma.Flows.Monitoring.Monitors;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Flows.Monitoring.Mongo
{
    public static class Extensions
    {
        public static IServiceCollection AddAlmaFlowsMonitoringMongo(this IServiceCollection services)
        {
            services.AddScoped<IValueMonitor, MongoValueMonitor>();
            return services;
        }
    }
}