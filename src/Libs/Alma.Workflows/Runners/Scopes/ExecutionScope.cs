using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.Scopes
{
    public interface IExecutionScope : IDisposable
    {
        void Initialize();

        IServiceScope Current { get; }
        bool HasCurrentScope { get; }
    }

    public class ExecutionScope : IExecutionScope
    {
        private readonly ILogger<ExecutionScope> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private IServiceScope? _currentScope;

        public ExecutionScope(ILogger<ExecutionScope> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public IServiceScope Current => GetCurrentScope();

        public bool HasCurrentScope => _currentScope is not null;

        public void Initialize()
        {
            _currentScope = _scopeFactory.CreateScope();
        }

        private IServiceScope GetCurrentScope()
        {
            return _currentScope ?? throw new InvalidOperationException("No current scope set.");
        }

        public void Dispose()
        {
            _currentScope?.Dispose();
        }
    }
}