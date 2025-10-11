using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Alma.Modules.Dashboards.Interop
{
    public interface IGridstackInterop : IAsyncDisposable
    {
        ValueTask InitializeAsync(object? options = null);
    }

    public class GridStackInterop : IGridstackInterop
    {
        private readonly ILogger<GridStackInterop> _logger;
        private readonly IJSRuntime _js;
        private IJSObjectReference? _module;

        public GridStackInterop(ILogger<GridStackInterop> logger, IJSRuntime jsRuntime)
        {
            _logger = logger;
            _js = jsRuntime;
        }

        public async ValueTask InitializeAsync(object? options = null)
        {
            _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "./_content/Alma.Modules.Dashboards/gridstackInterop.js");

            // Não retorna referência JS (usamos funções por seletor no JS)
            await _module.InvokeVoidAsync("initializeGridStack", options ?? new { });
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_module is not null)
                {
                    await _module.DisposeAsync();
                    _module = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error disposing GridStackInterop module");
            }
        }
    }
}