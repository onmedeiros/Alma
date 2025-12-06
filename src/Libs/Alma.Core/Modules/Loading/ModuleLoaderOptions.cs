namespace Alma.Core.Modules.Loading;

/// <summary>
/// Configuration options for external module loading.
/// </summary>
public class ModuleLoaderOptions
{
    /// <summary>
    /// Path to the directory containing external module DLLs.
    /// Can be absolute or relative to the application base directory.
    /// </summary>
    public string? ExternalModulesPath { get; set; }

    /// <summary>
    /// Enable file watching and hot-reload of modules when DLLs change.
    /// Note: Hot-reload requires application components to handle reload notifications.
    /// </summary>
    public bool EnableHotReload { get; set; } = false;

    /// <summary>
    /// Pattern to match module DLL files. Default is "*.dll".
    /// </summary>
    public string ModuleFilePattern { get; set; } = "*.dll";

    /// <summary>
    /// Debounce time in milliseconds for file change events during hot-reload.
    /// </summary>
    public int HotReloadDebounceMs { get; set; } = 1000;
}
