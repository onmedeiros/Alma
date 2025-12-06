namespace Alma.Core.Modules.Loading;

/// <summary>
/// Event args for module reload events.
/// </summary>
public class ModuleReloadEventArgs : EventArgs
{
    public required string ModulePath { get; init; }
    public required string ModuleName { get; init; }
    public ModuleReloadReason Reason { get; init; }
}

public enum ModuleReloadReason
{
    FileChanged,
    FileCreated,
    FileDeleted,
    ManualReload
}

/// <summary>
/// Interface for notifying clients about module reloads.
/// </summary>
public interface IModuleReloadNotifier
{
    /// <summary>
    /// Event raised when a module is reloaded.
    /// </summary>
    event EventHandler<ModuleReloadEventArgs>? ModuleReloaded;

    /// <summary>
    /// Notifies all subscribers that a module was reloaded.
    /// </summary>
    void NotifyReload(ModuleReloadEventArgs args);
}

/// <summary>
/// Default implementation of module reload notifier.
/// </summary>
public class ModuleReloadNotifier : IModuleReloadNotifier
{
    public event EventHandler<ModuleReloadEventArgs>? ModuleReloaded;

    public void NotifyReload(ModuleReloadEventArgs args)
    {
        ModuleReloaded?.Invoke(this, args);
    }
}
