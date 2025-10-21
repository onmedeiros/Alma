using Alma.Core.Data;
using Alma.Workflows.Alerts.Activities;
using Alma.Workflows.Alerts.Entities;
using Alma.Workflows.Alerts.Services;
using Alma.Workflows.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows.Alerts
{
    public static class Extensions
    {
        public static IServiceCollection AddFlowAlerts(this IServiceCollection services)
        {
            services.Configure<FlowOptions>(options =>
            {
                options.AddActivity<AlertActivity>();
            });

            services.AddScoped<IAlertService, AlertService>();

            services.ConfigureContext(options => options.AddIndex<Alert>(EntityIndexType.Ascending, ["OrganizationId"]));

            return services;
        }
    }
}