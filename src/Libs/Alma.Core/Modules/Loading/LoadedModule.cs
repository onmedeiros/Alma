using System.Reflection;

namespace Alma.Core.Modules.Loading;

/// <summary>
/// Represents a loaded external module with its context.
/// </summary>
public class LoadedModule
{
    public required string FilePath { get; init; }
    public required Assembly Assembly { get; init; }
    public required ModuleLoadContext LoadContext { get; init; }
    public required IModule Module { get; init; }
    public DateTime LoadedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Unloads the module and its assembly context.
    /// </summary>
    public void Unload()
    {
        LoadContext.Unload();
    }
}
