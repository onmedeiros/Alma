using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Monitoring.Activities;
using Alma.Workflows.Monitoring.MonitoringObjects.Services;
using Alma.Workflows.Monitoring.MonitoringObjectSchemas.ParameterProviders;
using Alma.Workflows.Monitoring.MonitoringObjectSchemas.Services;
using Alma.Workflows.Options;
using Microsoft.Extensions.DependencyInjection;
using Alma.Core;
using Alma.Core.Data;
using Alma.Workflows.Monitoring.MonitoringObjects.Entities;
using Alma.Workflows.Monitoring.MonitoringObjectSchemas.Entities;

namespace Alma.Workflows.Monitoring
{
    public static class Extensions
    {
        public static IServiceCollection AddAlmaWorkflowsMonitoring(this IServiceCollection services)
        {
            services.Configure<FlowOptions>(options =>
            {
                options.AddActivity<CreateMonitoringObjectActivity>();
                options.AddActivity<AverageMonitoringActivity>();
                options.AddActivity<CountMonitoringActivity>();
            });

            services.AddScoped<IMonitoringObjectService, MonitoringObjectService>();
            services.AddScoped<IMonitoringObjectSchemaService, MonitoringObjectSchemaService>();
            services.AddKeyedScoped<IParameterProvider, MonitoringObjectSchemaParameterProvider>(typeof(MonitoringObjectSchemaParameterProvider));

            services.AddSimpleValidator("Alma.Workflows.Monitoring");

            // Configure Context
            services.ConfigureContext(options => options
                .AddIndex<MonitoringObject>(EntityIndexType.Ascending, ["OrganizationId"])
                .AddIndex<MonitoringObject>(EntityIndexType.Ascending, ["SchemaId"])
                .AddIndex<MonitoringObject>(EntityIndexType.Ascending, ["PrimaryKey"])
                .AddIndex<MonitoringObject>(EntityIndexType.Descending, ["Timestamp"])
                .AddIndex<MonitoringObjectSchema>(EntityIndexType.Ascending, ["OrganizationId"])
            );

            return services;
        }
    }
}