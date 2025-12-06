using Alma.Core.Modules;
using Alma.Core.Modules.Loading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        /// <summary>
        /// Loads external modules from the specified directory path.
        /// Each DLL containing an IModule implementation will be loaded in isolation.
        /// </summary>
        /// <param name="path">Path to the directory containing module DLLs. Can be absolute or relative to application base.</param>
        /// <param name="configure">Optional configuration for the module loader.</param>
        public ModuleRegistryBuilder LoadFromDirectory(string path, Action<ModuleLoaderOptions>? configure = null)
        {
            var options = new ModuleLoaderOptions { ExternalModulesPath = path };
            configure?.Invoke(options);

            // Register options
            Services.Configure<ModuleLoaderOptions>(opt =>
            {
                opt.ExternalModulesPath = options.ExternalModulesPath;
                opt.EnableHotReload = options.EnableHotReload;
                opt.ModuleFilePattern = options.ModuleFilePattern;
                opt.HotReloadDebounceMs = options.HotReloadDebounceMs;
            });

            // Register ModuleLoader as singleton
            Services.AddSingleton<IModuleLoader, ModuleLoader>();

            // Build a temporary service provider to use ModuleLoader during configuration
            using var tempProvider = Services.BuildServiceProvider();
            var loader = tempProvider.GetRequiredService<IModuleLoader>();

            // Load modules from the directory
            var loadedModules = loader.LoadModulesFromDirectory(path).ToList();

            foreach (var loadedModule in loadedModules)
            {
                // Configure module services
                loadedModule.Module.Configure(Services);

                // Add the module instance to the configuration
                Services.Configure<ModuleOptions>(opt => opt.AddModule(loadedModule.Module));
            }

            // If hot-reload is enabled, register the watcher service
            if (options.EnableHotReload)
            {
                Services.AddSingleton<IModuleReloadNotifier, ModuleReloadNotifier>();
                Services.AddSingleton<IModuleWatcher, ModuleWatcher>();
                Services.AddHostedService<ModuleWatcherHostedService>();
            }

            return this;
        }
    }
}
