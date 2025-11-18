using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Databases.Activities;
using Alma.Workflows.Databases.Providers;
using Alma.Workflows.Databases.Registry;
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

            services.AddScoped<IDatabaseProviderRegistry, DatabaseProviderRegistry>();
            services.AddScoped<IDatabaseProvider, MongoDBDatabaseProvider>();

            services.AddKeyedScoped<IParameterProvider, DatabaseProviderParameterProvider>(typeof(DatabaseProviderParameterProvider));

            return services;
        }
    }
}