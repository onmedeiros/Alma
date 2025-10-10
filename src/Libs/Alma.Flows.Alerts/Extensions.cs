using Alma.Core.Data;
using Alma.Flows.Alerts.Activities;
using Alma.Flows.Alerts.Entities;
using Alma.Flows.Alerts.Services;
using Alma.Flows.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Flows.Alerts
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