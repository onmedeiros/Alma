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
using System.Runtime.Loader;

namespace Alma.Core
{
    public static class CoreExtensions
    {
        private static readonly HashSet<string> s_registeredAssemblies = new();
        public static ModuleRegistryBuilder AddModules(this IServiceCollection services, Action<ModuleOptions> configure)
        {
            services.Configure(configure);

            services.AddSingleton<IModuleRegistry, ModuleRegistry>();

            return new ModuleRegistryBuilder(services);
        }

        public static RazorComponentsEndpointConventionBuilder AddModules(this RazorComponentsEndpointConventionBuilder builder, IEndpointRouteBuilder endpoints)
        {
            var moduleRegistry = endpoints.ServiceProvider.GetRequiredService<IModuleRegistry>();

            // Avoid adding duplicate assemblies or the app's entry assembly, which is already registered.
            var entryAssembly = Assembly.GetEntryAssembly();
            var defaultAssemblies = AssemblyLoadContext.Default.Assemblies.ToList();

            var additionalAssemblies = moduleRegistry.Modules
                .Select(m => m.GetType().Assembly)
                .Select(a =>
                {
                    // Prefer the assembly from Default context if a matching name exists
                    var match = defaultAssemblies.FirstOrDefault(d => d.GetName().Name == a.GetName().Name);
                    return match ?? a;
                })
                .Where(a => a != null && a != entryAssembly)
                // Deduplicate by MVID (module version id) and FullName to avoid duplicates across contexts
                .GroupBy(a => new { Name = a.FullName, Mvid = a.ManifestModule.ModuleVersionId })
                .Select(g => g.First())
                // Exclude assemblies we already registered in previous calls
                .Where(a => s_registeredAssemblies.Add(a.FullName))
                .ToArray();

            if (additionalAssemblies.Length > 0)
            {
                builder.AddAdditionalAssemblies(additionalAssemblies);
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