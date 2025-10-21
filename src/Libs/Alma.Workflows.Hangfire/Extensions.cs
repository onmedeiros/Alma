using Alma.Workflows.Core.InstanceSchedules.Services;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows.Hangfire
{
    public static class Extensions
    {
        public static void AddWorkflowsHangfire(this IServiceCollection services)
        {
            // Replace services
            services.AddScoped<IInstanceScheduleJobManager, HangfireInstanceScheduleJobManager>();

            // Add Hangfire server

            services.AddHangfireServer(options =>
            {
                options.ServerName = "Alma.Workflows";
                options.Queues = new[] { "Alma-Workflows-once", "Alma-Workflows-recurring" };
            });
        }
    }
}