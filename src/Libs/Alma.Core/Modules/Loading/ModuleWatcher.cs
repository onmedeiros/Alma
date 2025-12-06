using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alma.Core.Modules.Loading;

/// <summary>
/// Interface for watching module directory for changes.
/// </summary>
public interface IModuleWatcher : IDisposable
{
    /// <summary>
    /// Starts watching the configured directory for changes.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops watching for changes.
    /// </summary>
    void Stop();

    /// <summary>
    /// Gets whether the watcher is currently active.
    /// </summary>
    bool IsWatching { get; }
}

/// <summary>
/// Watches the external modules directory for changes and triggers reloads.
/// </summary>
public class ModuleWatcher : IModuleWatcher
{
    private readonly ILogger<ModuleWatcher> _logger;
    private readonly ModuleLoaderOptions _options;
    private readonly IModuleLoader _moduleLoader;
    private readonly IModuleReloadNotifier _reloadNotifier;

    private FileSystemWatcher? _watcher;
    private readonly Dictionary<string, DateTime> _lastChangeTimes = [];
    private readonly object _lock = new();
    private bool _disposed;

    public bool IsWatching => _watcher?.EnableRaisingEvents ?? false;

    public ModuleWatcher(
        ILogger<ModuleWatcher> logger,
        IOptions<ModuleLoaderOptions> options,
        IModuleLoader moduleLoader,
        IModuleReloadNotifier reloadNotifier)
    {
        _logger = logger;
        _options = options.Value;
        _moduleLoader = moduleLoader;
        _reloadNotifier = reloadNotifier;
    }

    public void Start()
    {
        if (string.IsNullOrEmpty(_options.ExternalModulesPath))
        {
            _logger.LogWarning("Cannot start module watcher: ExternalModulesPath is not configured");
            return;
        }

        var absolutePath = Path.IsPathRooted(_options.ExternalModulesPath)
            ? _options.ExternalModulesPath
            : Path.Combine(AppContext.BaseDirectory, _options.ExternalModulesPath);

        if (!Directory.Exists(absolutePath))
        {
            _logger.LogWarning("Cannot start module watcher: Directory does not exist: {Path}", absolutePath);
            return;
        }

        _watcher = new FileSystemWatcher(absolutePath)
        {
            Filter = _options.ModuleFilePattern,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileCreated;
        _watcher.Deleted += OnFileDeleted;
        _watcher.Error += OnError;

        _logger.LogInformation("Module watcher started for directory: {Path}", absolutePath);
    }

    public void Stop()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= OnFileChanged;
            _watcher.Created -= OnFileCreated;
            _watcher.Deleted -= OnFileDeleted;
            _watcher.Error -= OnError;
            _watcher.Dispose();
            _watcher = null;

            _logger.LogInformation("Module watcher stopped");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (!ShouldProcess(e.FullPath)) return;

        _logger.LogInformation("Module file changed: {Path}", e.FullPath);
        HandleModuleChange(e.FullPath, ModuleReloadReason.FileChanged);
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        if (!ShouldProcess(e.FullPath)) return;

        _logger.LogInformation("New module file detected: {Path}", e.FullPath);
        HandleModuleChange(e.FullPath, ModuleReloadReason.FileCreated);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Module file deleted: {Path}", e.FullPath);

        var loadedModule = _moduleLoader.LoadedModules
            .FirstOrDefault(m => m.FilePath.Equals(e.FullPath, StringComparison.OrdinalIgnoreCase));

        if (loadedModule != null)
        {
            var moduleName = loadedModule.Module.Descriptor.Name;
            _moduleLoader.UnloadModule(loadedModule);

            _reloadNotifier.NotifyReload(new ModuleReloadEventArgs
            {
                ModulePath = e.FullPath,
                ModuleName = moduleName,
                Reason = ModuleReloadReason.FileDeleted
            });
        }
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        _logger.LogError(e.GetException(), "Module watcher error");
    }

    private bool ShouldProcess(string filePath)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;

            if (_lastChangeTimes.TryGetValue(filePath, out var lastChange))
            {
                if ((now - lastChange).TotalMilliseconds < _options.HotReloadDebounceMs)
                {
                    return false;
                }
            }

            _lastChangeTimes[filePath] = now;
            return true;
        }
    }

    private void HandleModuleChange(string filePath, ModuleReloadReason reason)
    {
        // Wait a bit to ensure the file is fully written
        Task.Delay(500).ContinueWith(_ =>
        {
            try
            {
                var existingModule = _moduleLoader.LoadedModules
                    .FirstOrDefault(m => m.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));

                LoadedModule? newModule;
                string moduleName;

                if (existingModule != null)
                {
                    moduleName = existingModule.Module.Descriptor.Name;
                    newModule = _moduleLoader.ReloadModule(existingModule);
                }
                else
                {
                    newModule = _moduleLoader.LoadModule(filePath);
                    moduleName = newModule?.Module.Descriptor.Name ?? Path.GetFileNameWithoutExtension(filePath);
                }

                if (newModule != null)
                {
                    _logger.LogInformation("Module reloaded successfully: {Module}", newModule.Module.Descriptor.Name);

                    _reloadNotifier.NotifyReload(new ModuleReloadEventArgs
                    {
                        ModulePath = filePath,
                        ModuleName = newModule.Module.Descriptor.Name,
                        Reason = reason
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reload module: {Path}", filePath);
            }
        });
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
        }
    }
}
