using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Runtime.Loader;
using System.Linq;

namespace Alma.Core.Modules.Loading;

/// <summary>
/// Service responsible for loading external modules from a configured directory.
/// </summary>
public interface IModuleLoader
{
    /// <summary>
    /// Gets all currently loaded external modules.
    /// </summary>
    IReadOnlyList<LoadedModule> LoadedModules { get; }

    /// <summary>
    /// Scans the configured directory and loads all modules.
    /// </summary>
    IEnumerable<LoadedModule> LoadModulesFromDirectory(string path);

    /// <summary>
    /// Loads a single module from the specified DLL path.
    /// </summary>
    LoadedModule? LoadModule(string dllPath);

    /// <summary>
    /// Unloads a specific module.
    /// </summary>
    void UnloadModule(LoadedModule module);

    /// <summary>
    /// Reloads a module from the same path.
    /// </summary>
    LoadedModule? ReloadModule(LoadedModule module);
}

public class ModuleLoader : IModuleLoader
{
    private readonly ILogger<ModuleLoader> _logger;
    private readonly ModuleLoaderOptions _options;
    private readonly List<LoadedModule> _loadedModules = [];
    private readonly object _lock = new();

    public IReadOnlyList<LoadedModule> LoadedModules
    {
        get
        {
            lock (_lock)
            {
                return _loadedModules.AsReadOnly();
            }
        }
    }

    public ModuleLoader(ILogger<ModuleLoader> logger, IOptions<ModuleLoaderOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public IEnumerable<LoadedModule> LoadModulesFromDirectory(string path)
    {
        var absolutePath = Path.IsPathRooted(path)
            ? path
            : Path.Combine(AppContext.BaseDirectory, path);

        if (!Directory.Exists(absolutePath))
        {
            _logger.LogWarning("External modules directory does not exist: {Path}", absolutePath);
            yield break;
        }

        _logger.LogInformation("Scanning for external modules in: {Path}", absolutePath);

        var dllFiles = Directory.GetFiles(absolutePath, _options.ModuleFilePattern, SearchOption.TopDirectoryOnly);

        foreach (var dllFile in dllFiles)
        {
            var module = LoadModule(dllFile);
            if (module != null)
            {
                yield return module;
            }
        }
    }

    public LoadedModule? LoadModule(string dllPath)
    {
        if (!File.Exists(dllPath))
        {
            _logger.LogWarning("Module DLL not found: {Path}", dllPath);
            return null;
        }

        try
        {
            _logger.LogDebug("Loading module from: {Path}", dllPath);

            var loadContext = new ModuleLoadContext(dllPath);
            var assembly = loadContext.LoadFromAssemblyPath(dllPath);

            // Find types implementing IModule
            var moduleTypes = assembly.GetTypes()
                .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            if (moduleTypes.Count == 0)
            {
                _logger.LogDebug("No IModule implementations found in: {Path}", dllPath);
                loadContext.Unload();
                return null;
            }

            if (moduleTypes.Count > 1)
            {
                _logger.LogWarning("Multiple IModule implementations found in {Path}. Using first: {Type}",
                    dllPath, moduleTypes[0].FullName);
            }

            var moduleType = moduleTypes[0];
            var moduleInstance = (IModule?)Activator.CreateInstance(moduleType);

            if (moduleInstance == null)
            {
                _logger.LogError("Failed to create instance of module {Type} from {Path}", moduleType.FullName, dllPath);
                loadContext.Unload();
                return null;
            }

            var loadedModule = new LoadedModule
            {
                FilePath = dllPath,
                Assembly = assembly,
                LoadContext = loadContext,
                Module = moduleInstance
            };

            lock (_lock)
            {
                _loadedModules.Add(loadedModule);
            }

            _logger.LogInformation("Successfully loaded external module: {Module} from {Path}",
                moduleInstance.Descriptor.Name, dllPath);

            return loadedModule;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load module from: {Path}", dllPath);
            return null;
        }
    }

    public void UnloadModule(LoadedModule module)
    {
        lock (_lock)
        {
            if (_loadedModules.Remove(module))
            {
                _logger.LogInformation("Unloading module: {Module}", module.Module.Descriptor.Name);
                module.Unload();

                // Force garbage collection to actually unload the assembly
                for (var i = 0; i < 10 && module.LoadContext.Assemblies.Any(); i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
    }


    public LoadedModule? ReloadModule(LoadedModule module)
    {
        var path = module.FilePath;
        UnloadModule(module);

        // Small delay to ensure file is released
        Thread.Sleep(100);

        return LoadModule(path);
    }
}