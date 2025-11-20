using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Core.InstanceExecutions.Enums;
using Alma.Workflows.Core.Instances.Entities;
using Alma.Workflows.Core.Instances.Services;
using Alma.Workflows.Options;
using Alma.Workflows.Parsers;
using Alma.Workflows.Runners;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Core.InstanceExecutions.Services
{
    public interface IInstanceExecutionRunner
    {
        ValueTask<FlowExecutionContext> ExecuteAsync(string instanceId, string? discriminator = null, ExecutionOptions? options = null);

        ValueTask<FlowExecutionContext> ExecuteAsync(FlowInstance instance, Flow flow, ExecutionOptions? options = null);
    }

    public class InstanceExecutionRunner : IInstanceExecutionRunner
    {
        private readonly ILogger<InstanceExecutionRunner> _logger;
        private readonly IFlowManager _flowManager;
        private readonly IFlowDefinitionParser _flowDefinitionParser;
        private readonly IInstanceManager _flowInstanceManager;
        private readonly IInstanceExecutionManager _instanceExecutionManager;
        private readonly IWorkflowRunnerFactory _flowRunnerFactory;

        public InstanceExecutionRunner(ILogger<InstanceExecutionRunner> logger, IFlowManager flowManager, IFlowDefinitionParser flowDefinitionParser, IInstanceManager flowInstanceManager, IInstanceExecutionManager instanceExecutionManager, IWorkflowRunnerFactory flowRunnerFactory)
        {
            _logger = logger;
            _flowManager = flowManager;
            _flowDefinitionParser = flowDefinitionParser;
            _flowInstanceManager = flowInstanceManager;
            _instanceExecutionManager = instanceExecutionManager;
            _flowRunnerFactory = flowRunnerFactory;
        }

        public async ValueTask<FlowExecutionContext> ExecuteAsync(string instanceId, string? discriminator = null, ExecutionOptions? options = null)
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
            return await ExecuteAsync(instance, flow, options);
        }

        public async ValueTask<FlowExecutionContext> ExecuteAsync(FlowInstance instance, Flow flow, ExecutionOptions? options = null)
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