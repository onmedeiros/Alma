using Alma.Workflows.Monitoring.Monitors;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows.Monitoring.Mongo
{
    public static class Extensions
    {
        public static IServiceCollection AddAlmaWorkflowsMonitoringMongo(this IServiceCollection services)
        {
            services.AddScoped<IValueMonitor, MongoValueMonitor>();
            services.AddScoped<IMonitoringObjectMonitor, MongoMonitoringObjectMonitor>();

            return services;
        }
    }
}