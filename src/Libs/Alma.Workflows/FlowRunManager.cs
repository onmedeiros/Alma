using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Core.InstanceExecutions.Enums;
using Alma.Workflows.Core.InstanceExecutions.Services;
using Alma.Workflows.Core.Instances.Entities;
using Alma.Workflows.Core.Instances.Services;
using Alma.Workflows.Options;
using Alma.Workflows.Parsers;
using Alma.Workflows.Runners;
using Alma.Workflows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows
{
    [Obsolete("Use IInstanceExecutor instead.")]
    public interface IFlowRunManager
    {
        [Obsolete]
        ValueTask<FlowExecutionContext> RunAsync(Flow flow, ExecutionState? state);

        [Obsolete]
        ValueTask<FlowExecutionContext> RunAsync(Flow flow, params KeyValuePair<string, object?>[] parameters);

        [Obsolete]
        ValueTask<FlowExecutionContext> RunAsync(Flow flow, ExecutionOptions? options = null, Dictionary<string, object?>? parameters = null);

        ValueTask<FlowExecutionContext> RunAsync(string instanceId, string? discriminator = null, ExecutionOptions? options = null);

        ValueTask<FlowExecutionContext> RunAsync(FlowInstance instance, Flow flow, ExecutionOptions? options = null);
    }

    [Obsolete("Use InstanceExecutor instead.")]
    public class FlowRunManager : IFlowRunManager
    {
        private readonly ILogger<FlowRunManager> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFlowManager _flowManager;
        private readonly IFlowDefinitionParser _flowDefinitionParser;
        private readonly IInstanceManager _flowInstanceManager;
        private readonly IInstanceExecutionManager _instanceExecutionManager;
        private readonly IFlowRunnerFactory _flowRunnerFactory;

        public FlowRunManager(ILogger<FlowRunManager> logger, IServiceProvider serviceProvider, IFlowManager flowManager, IFlowDefinitionParser flowDefinitionParser, IInstanceManager flowInstanceManager, IInstanceExecutionManager instanceExecutionManager, IFlowRunnerFactory flowRunnerFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _flowManager = flowManager;
            _flowDefinitionParser = flowDefinitionParser;
            _flowInstanceManager = flowInstanceManager;
            _instanceExecutionManager = instanceExecutionManager;
            _flowRunnerFactory = flowRunnerFactory;
        }

        [Obsolete]
        public async ValueTask<FlowExecutionContext> RunAsync(Flow flow, ExecutionState? state)
        {
            var context = new FlowExecutionContext(flow, _serviceProvider)
            {
                Id = flow.Id
            };

            await flow.ExecuteAsync(context);

            return context;
        }

        [Obsolete]
        public async ValueTask<FlowExecutionContext> RunAsync(Flow flow, params KeyValuePair<string, object?>[] parameters)
        {
            var context = new FlowExecutionContext(flow, _serviceProvider)
            {
                Id = flow.Id
            };

            foreach (var parameter in parameters)
            {
                context.State.Parameters.Add(parameter.Key, parameter.Value);
            }

            await flow.ExecuteAsync(context);

            return context;
        }

        [Obsolete]
        public async ValueTask<FlowExecutionContext> RunAsync(Flow flow, ExecutionOptions? options = null, Dictionary<string, object?>? parameters = null)
        {
            var context = new FlowExecutionContext(flow, _serviceProvider)
            {
                Id = flow.Id,
                Options = options ?? new ExecutionOptions()
            };

            if (parameters is not null)
            {
                foreach (var parameter in parameters)
                {
                    context.State.Parameters.Add(parameter.Key, parameter.Value);
                }
            }

            await flow.ExecuteAsync(context);

            return context;
        }

        public async ValueTask<FlowExecutionContext> RunAsync(string instanceId, string? discriminator = null, ExecutionOptions? options = null)
        {
            // Load and validate instance
            var instance = await _flowInstanceManager.FindById(instanceId, discriminator);

            if (instance is null)
            {
                _logger.LogError("Flow instance with id {Id} not found.", instanceId);
                throw new Exception($"Flow instance with id {instanceId} not found.");
            }

            if (string.IsNullOrEmpty(instance.FlowDefinitionVersionId))
            {
                _logger.LogError("Flow instance with id {Id} has no flow definition version selected.", instanceId);
                throw new Exception($"Flow instance with id {instanceId} has no flow definition version selected.");
            }

            if (!instance.IsActive)
            {
                _logger.LogError("Flow instance with id {Id} is not active.", instanceId);
                throw new Exception($"Flow instance with id {instanceId} is not active.");
            }

            // Load and validate flow
            var definitionVersion = await _flowManager.FindDefinitionVersionById(instance.FlowDefinitionVersionId);

            if (definitionVersion is null)
            {
                _logger.LogError("Flow definition version with id {Id} not found.", instance.FlowDefinitionVersionId);
                throw new Exception($"Flow definition version with id {instance.FlowDefinitionVersionId} not found.");
            }

            var parsed = _flowDefinitionParser.TryParse(definitionVersion.FlowDefinition, out var flow);

            if (!parsed)
            {
                _logger.LogError("Flow definition version with id {Id} could not be parsed.", instance.FlowDefinitionVersionId);
                throw new Exception($"Flow definition version with id {instance.FlowDefinitionVersionId} could not be parsed.");
            }

            // Begin run
            return await RunAsync(instance, flow, options);
        }

        public async ValueTask<FlowExecutionContext> RunAsync(FlowInstance instance, Flow flow, ExecutionOptions? options = null)
        {
            // Inicia a execução
            var instanceExecution = await _instanceExecutionManager.Begin(instance, options);

            // Cria uma instância do runner
            var runner = _flowRunnerFactory.Create(flow, instanceExecution.State, instanceExecution.Options);

            // Define o tempo limite de execução (exemplo: 30 segundos)
            var timeout = TimeSpan.FromSeconds(30);
            using var cancellationTokenSource = new CancellationTokenSource(timeout);

            try
            {
                // Há dois modos de execução: Automático e manual.
                // O modo automático executa até o final, enquanto o modo manual executa passo a passo.
                if (runner.Context.Options.ExecutionMode == InstanceExecutionMode.Automatic)
                {
                    // Executa até o final ou até o tempo limite ser atingido
                    while (await runner.ExecuteNextAsync())
                    {
                        cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                }
                else
                {
                    // Executa passo a passo
                    await runner.ExecuteNextAsync();
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "A execução do flow foi cancelada devido ao tempo limite.");
                throw new TimeoutException("A execução do flow foi cancelada devido ao tempo limite.");
            }

            // Ao terminar, atualiza a entidade da execução da instância
            await _instanceExecutionManager.Update(instanceExecution, runner.Context.State);

            return runner.Context;
        }
    }
}