using Alma.Modules.Widgets.Interop;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace Alma.Modules.Dashboards.Interop
{
    public interface IGridstackInterop : IAsyncDisposable
    {
        event Func<GridStackInteropEventArgs, Task>? OnAdded;

        event Func<GridStackInteropEventArgs, Task>? OnChanged;

        event Func<GridStackInteropEventArgs, Task>? OnRemoved;

        ValueTask InitializeAsync(GridstackInteropOptions? options = null);

        ValueTask SetStatic(bool opt);

        ValueTask MakeWidget(string id);

        ValueTask RemoveWidget(string id);
    }

    public class GridStackInterop : IGridstackInterop
    {
        private readonly ILogger<GridStackInterop> _logger;
        private readonly IJSRuntime _js;
        private IJSObjectReference? _module;
        private IJSObjectReference? _grid;

        public GridStackInterop(ILogger<GridStackInterop> logger, IJSRuntime jsRuntime)
        {
            _logger = logger;
            _js = jsRuntime;
        }

        public async ValueTask InitializeAsync(GridstackInteropOptions? options = null)
        {
            var interopReference = DotNetObjectReference.Create(this);

            _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "./_content/Alma.Modules.Widgets/gridstackInterop.js");
            _grid = await _module.InvokeAsync<IJSObjectReference>("initializeGridStack", options ?? new GridstackInteropOptions(), interopReference);
        }

        public ValueTask SetStatic(bool opt)
        {
            EnsureInitialized(_grid);

            return _grid.InvokeVoidAsync("setStatic", opt);
        }

        public ValueTask MakeWidget(string id)
        {
            EnsureInitialized(_grid);

            return _grid.InvokeVoidAsync("makeWidget", $"#id-{id}");
        }

        public ValueTask RemoveWidget(string id)
        {
            EnsureInitialized(_grid);
            return _grid.InvokeVoidAsync("removeWidget", $"#id-{id}");
        }

        #region Events

        public event Func<GridStackInteropEventArgs, Task>? OnAdded;

        public event Func<GridStackInteropEventArgs, Task>? OnChanged;

        public event Func<GridStackInteropEventArgs, Task>? OnRemoved;

        #endregion

        #region JS Events

        [JSInvokable("HandleAdded")]
        public async Task HandleAdded(GridStackInteropEventArgs args)
        {
            if (OnAdded is not null)
            {
                await OnAdded.Invoke(args);
            }
        }

        [JSInvokable("HandleChanged")]
        public async Task HandleChanged(GridStackInteropEventArgs args)
        {
            if (OnChanged is not null)
            {
                await OnChanged.Invoke(args);
            }
        }

        [JSInvokable("HandleRemoved")]
        public async Task HandleRemoved(GridStackInteropEventArgs args)
        {
            if (OnRemoved is not null)
            {
                await OnRemoved.Invoke(args);
            }
        }

        #endregion

        #region Private

        private void EnsureInitialized([NotNull] IJSObjectReference? grid)
        {
            if (grid is null)
            {
                throw new InvalidOperationException("GridStackInterop is not initialized. Call InitializeAsync first.");
            }
        }

        #endregion

        #region Disposing

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_module is not null)
                {
                    await _module.DisposeAsync();
                    _module = null;
                }

                if (_grid is not null)
                {
                    await _grid.DisposeAsync();
                    _grid = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error disposing GridStackInterop module");
            }
        }

        #endregion
    }
}