using System.Reflection;
using System.Runtime.Loader;
using System.Linq;

namespace Alma.Core.Modules.Loading;

/// <summary>
/// Custom AssemblyLoadContext for loading external modules with isolation.
/// Supports unloading for hot-reload scenarios.
/// </summary>
public class ModuleLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public string ModulePath { get; }

    public ModuleLoadContext(string modulePath) : base(isCollectible: true)
    {
        ModulePath = modulePath;
        _resolver = new AssemblyDependencyResolver(modulePath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Prefer the default context for shared assemblies to preserve type identity
        // across the host and plugin (e.g., Alma.Core containing IModule).
        var modulesToIgnore = new List<string>
        {
             "Alma.Core",
             "Alma.Modules.Base",
             "Alma.Core.Mongo",
             "Alma.Modules.Core",
             "Alma.Organizations"
        };

        if (modulesToIgnore.Contains(assemblyName.Name ?? string.Empty))
        {
            // If already loaded in default, reuse it; otherwise load by name into default.
            var shared = Default.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
            return shared ?? Assembly.Load(assemblyName);
        }

        // First, try to resolve from the module's directory
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        // If not found locally, let the default context handle it
        // This allows sharing of common assemblies like Alma.Core
        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}