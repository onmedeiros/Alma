using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Alma.Core.Modules
{
    public interface IModuleRegistry
    {
        IEnumerable<Assembly> Assemblies { get; }
        IEnumerable<IModule> Modules { get; }
    }

    public class ModuleRegistry : IModuleRegistry
    {
        private readonly ILogger<ModuleRegistry> _logger;
        private readonly ModuleOptions _options;

        private readonly ICollection<IModule> _modules = [];

        public IEnumerable<Assembly> Assemblies => _options.Assemblies;
        public IEnumerable<IModule> Modules => _modules;

        public ModuleRegistry(ILogger<ModuleRegistry> logger, IOptions<ModuleOptions> options)
        {
            _logger = logger;
            _options = options.Value;

            // Register modules from the assemblies.
            foreach (var assembly in _options.Assemblies)
                RegisterModule(assembly);

            // Register modules from the options.
            foreach (var module in _options.Modules)
                RegisterModule(module);
        }

        public void RegisterModule(Assembly assembly)
        {
            // Search for types that implement IModule in the assembly.
            var moduleTypes = assembly.GetTypes()
                .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            // If no modules found, log a warning and return.
            if (!moduleTypes.Any())
            {
                _logger.LogWarning("No modules found in assembly {Assembly}.", assembly.FullName);
                return;
            }

            // Register each module found.
            foreach (var type in moduleTypes)
            {
                try
                {
                    // Create an instance of the module.
                    var moduleInstance = (IModule?)Activator.CreateInstance(type);

                    // If the module instance is null, log an error and throw an exception.
                    if (moduleInstance == null)
                    {
                        _logger.LogError("Failed to create instance of module {Module}.", type.FullName);
                        throw new Exception($"Failed to create instance of module {type.FullName}.");
                    }

                    // Add the module instance to the collection.
                    _modules.Add(moduleInstance);
                    _logger.LogInformation("Module {module} registered successfully.", type.FullName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to register module {Module}.", type.FullName);
                }
            }
        }

        public void RegisterModule(IModule module)
        {
            _modules.Add(module);
            _logger.LogInformation("Module {module} registered successfully.", module.GetType().FullName);
        }
    }
}
