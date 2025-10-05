using Alma.Core.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Core.Builders
{
    public class ModuleRegistryBuilder
    {
        public IServiceCollection Services { get; }

        public ModuleRegistryBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public ModuleRegistryBuilder Register<TModule>() where TModule : IModule
        {
            return Register(typeof(TModule));
        }

        public ModuleRegistryBuilder Register(Type type)
        {
            // Check if the type implements IModule.
            if (!typeof(IModule).IsAssignableFrom(type))
            {
                throw new ArgumentException("Type must implement IModule.", nameof(type));
            }

            // Create an instance of the module.
            var moduleInstance = (IModule?)Activator.CreateInstance(type);

            // If the module instance is null, log an error and throw an exception.
            if (moduleInstance == null)
            {
                throw new Exception($"Failed to create instance of module {type.FullName}.");
            }

            // Configure module services.
            moduleInstance.Configure(Services);

            // Add the module instance to the configuration.
            Services.Configure<ModuleOptions>(options => options.AddModule(moduleInstance));

            return this;
        }
    }
}
