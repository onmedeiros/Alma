using Alma.Workflows.Databases.Activities;
using Alma.Workflows.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows.Databases
{
    public static class Extensions
    {
        public static IServiceCollection AddWorkflowDatabases(this IServiceCollection services)
        {
            services.Configure<FlowOptions>(options =>
            {
                options.AddActivity<QueryDatabaseActivity>();
            });

            return services;
        }
    }
}