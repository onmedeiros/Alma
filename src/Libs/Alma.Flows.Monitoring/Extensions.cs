using Alma.Flows.Core.Activities.Abstractions;
using Alma.Flows.Monitoring.Activities;
using Alma.Flows.Monitoring.MonitoringObjects.Services;
using Alma.Flows.Monitoring.MonitoringObjectSchemas.ParameterProviders;
using Alma.Flows.Monitoring.MonitoringObjectSchemas.Services;
using Alma.Flows.Options;
using Microsoft.Extensions.DependencyInjection;
using Alma.Core;

namespace Alma.Flows.Monitoring
{
    public static class Extensions
    {
        public static IServiceCollection AddAlmaFlowsMonitoring(this IServiceCollection services)
        {
            services.Configure<FlowOptions>(options =>
            {
                options.AddActivity<CreateMonitoringObjectActivity>();
                options.AddActivity<AverageMonitoringActivity>();
            });

            services.AddScoped<IMonitoringObjectService, MonitoringObjectService>();
            services.AddScoped<IMonitoringObjectSchemaService, MonitoringObjectSchemaService>();
            services.AddKeyedScoped<IParameterProvider, MonitoringObjectSchemaParameterProvider>(typeof(MonitoringObjectSchemaParameterProvider));

            services.AddSimpleValidator("Alma.Flows.Monitoring");

            return services;
        }
    }
}