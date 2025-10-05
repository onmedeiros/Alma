using Alma.Core.Builders;
using Alma.Core.Contexts;
using Alma.Core.Modules;
using Alma.Core.Validations;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using System.Reflection;

namespace Alma.Core
{
    public static class CoreExtensions
    {
        public static ModuleRegistryBuilder AddModules(this IServiceCollection services, Action<ModuleOptions> configure)
        {
            services.Configure(configure);

            services.AddSingleton<IModuleRegistry, ModuleRegistry>();

            return new ModuleRegistryBuilder(services);
        }

        public static RazorComponentsEndpointConventionBuilder AddModules(this RazorComponentsEndpointConventionBuilder builder, IEndpointRouteBuilder endpoints)
        {
            var moduleRegistry = endpoints.ServiceProvider.GetRequiredService<IModuleRegistry>();

            foreach (var module in moduleRegistry.Modules)
            {
                builder.AddAdditionalAssemblies(module.GetType().Assembly);
            }

            return builder;
        }

        public static IServiceCollection AddSimpleValidator(this IServiceCollection services, string assembly)
        {
            Log.Information("Adding SimpleValidator with assembly {Assembly}.", assembly);

            services.AddValidatorsFromAssembly(Assembly.Load(assembly));
            services.TryAddScoped<Validations.IValidator, Validator>();
            return services;
        }

        public static IServiceCollection AddUserContext(this IServiceCollection services)
        {
            services.AddScoped<IUserContext, UserContext>();
            return services;
        }
    }
}