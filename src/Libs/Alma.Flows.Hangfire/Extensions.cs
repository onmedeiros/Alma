using Alma.Flows.Core.InstanceSchedules.Services;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Flows.Hangfire
{
    public static class Extensions
    {
        public static void AddFlowsHangfire(this IServiceCollection services)
        {
            // Replace services
            services.AddScoped<IInstanceScheduleJobManager, HangfireInstanceScheduleJobManager>();

            // Add Hangfire server

            services.AddHangfireServer(options =>
            {
                options.ServerName = "Alma.Flows";
                options.Queues = new[] { "Alma-flows-once", "Alma-flows-recurring" };
            });
        }
    }
}