using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alma.Core.Modules.Loading;

/// <summary>
/// Hosted service that manages the module watcher lifecycle.
/// </summary>
public class ModuleWatcherHostedService : IHostedService
{
    private readonly ILogger<ModuleWatcherHostedService> _logger;
    private readonly IModuleWatcher _moduleWatcher;

    public ModuleWatcherHostedService(
        ILogger<ModuleWatcherHostedService> logger,
        IModuleWatcher moduleWatcher)
    {
        _logger = logger;
        _moduleWatcher = moduleWatcher;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting module watcher hosted service");
        _moduleWatcher.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping module watcher hosted service");
        _moduleWatcher.Stop();
        return Task.CompletedTask;
    }
}
