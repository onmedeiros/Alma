using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Core.InstanceExecutions.Entities;
using Alma.Workflows.Core.InstanceExecutions.Enums;
using Alma.Workflows.Core.Instances.Entities;
using Alma.Workflows.Core.Instances.Services;
using Alma.Workflows.Enums;
using Alma.Workflows.Options;
using Alma.Workflows.Parsers;
using Alma.Workflows.Runners;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Core.InstanceExecutions.Services
{
    public interface IInstanceRunner
    {
        ValueTask<WorkflowExecutionContext> ExecuteAsync(string instanceId, string? discriminator = null, ExecutionOptions? options = null);

        ValueTask<WorkflowExecutionContext> ExecuteAsync(Instance instance, ExecutionOptions? options = null);
    }

    public class InstanceRunner : IInstanceRunner
    {
        private readonly ILogger<InstanceRunner> _logger;
        private readonly IWorkflowManager _workflowManager;
        private readonly IFlowDefinitionParser _flowDefinitionParser;
        private readonly IInstanceManager _flowInstanceManager;
        private readonly IInstanceExecutionManager _instanceExecutionManager;
        private readonly IWorkflowRunnerFactory _flowRunnerFactory;

        public InstanceRunner(ILogger<InstanceRunner> logger, IWorkflowManager flowManager, IFlowDefinitionParser flowDefinitionParser, IInstanceManager flowInstanceManager, IInstanceExecutionManager instanceExecutionManager, IWorkflowRunnerFactory flowRunnerFactory)
        {
            _logger = logger;
            _workflowManager = flowManager;
            _flowDefinitionParser = flowDefinitionParser;
            _flowInstanceManager = flowInstanceManager;
            _instanceExecutionManager = instanceExecutionManager;
            _flowRunnerFactory = flowRunnerFactory;
        }

        public async ValueTask<WorkflowExecutionContext> ExecuteAsync(string instanceId, string? discriminator = null, ExecutionOptions? options = null)
        {
            // Load and validate instance
            var instance = await _flowInstanceManager.FindById(instanceId, discriminator);

            if (instance is null)
            {
                _logger.LogError("Flow instance with id {Id} not found.", instanceId);
                throw new Exception($"Flow instance with id {instanceId} not found.");
            }

            // Begin run
            return await ExecuteAsync(instance, options);
        }

        public async ValueTask<WorkflowExecutionContext> ExecuteAsync(Instance instance, ExecutionOptions? options = null)
        {
            options ??= new ExecutionOptions();

            // Load workflow by definition
            var workflow = await LoadWorkflow(instance);

            // Create instance execution entity
            var instanceExecution = await _instanceExecutionManager.Create(instance, options);

            // Create workflow runner
            var runner = _flowRunnerFactory.Create(workflow, instanceExecution.State, instanceExecution.Options);

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
            instanceExecution.State = runner.Context.State.StateData;
            instanceExecution.Status = GetInstanceExecutionStatus(runner.Context);

            if (options.Persist)
                await _instanceExecutionManager.Update(instanceExecution);

            return runner.Context;
        }

        private async ValueTask<Workflow> LoadWorkflow(Instance instance)
        {
            if (!string.IsNullOrEmpty(instance.WorkflowDefinitionVersionId))
            {
                return await LoadWorkflowByDefinitionVersion(instance.WorkflowDefinitionVersionId);
            }
            else if (!string.IsNullOrEmpty(instance.WorkflowDefinitionId))
            {
                return await LoadWorkflowByDefinitionId(instance.WorkflowDefinitionId);
            }
            else
            {
                _logger.LogError("Impossible to load workflow for Instance {InstanceId}. No definition or version specified.", instance.Id);
                throw new InvalidOperationException($"Impossible to load workflow for Instance {instance.Id}");
            }
        }

        private async ValueTask<Workflow> LoadWorkflowByDefinitionId(string definitionId)
        {
            var definition = await _workflowManager.FindDefinitionById(definitionId);

            if (definition is null)
            {
                _logger.LogError("Workflow definition with id {Id} not found.", definitionId);
                throw new Exception($"Workflow definition with id {definitionId} not found.");
            }

            var latestVersion = await _workflowManager.FindDefinitionById(definitionId);

            if (latestVersion is null)
            {
                _logger.LogError("Workflow definition with id {Id} not found.", definitionId);
                throw new Exception($"Workflow definition with id {definitionId} not found.");
            }

            return await LoadWorkflowByDefinitionVersion(latestVersion.Id);
        }

        private async ValueTask<Workflow> LoadWorkflowByDefinitionVersion(string versionId)
        {
            var definitionVersion = await _workflowManager.FindDefinitionVersionById(versionId);

            if (definitionVersion is null)
            {
                _logger.LogError("Workflow definition version with id {Id} not found.", versionId);
                throw new Exception($"Workflow definition version with id {versionId} not found.");
            }

            var parsedDefinition = _flowDefinitionParser.TryParse(definitionVersion.FlowDefinition, out var workflow);

            if (!parsedDefinition)
            {
                _logger.LogError("Failed to parse Workflow Definition for version {Id}.", versionId);
                throw new Exception($"Failed to parse Workflow Definition for version {versionId}.");
            }

            return workflow;
        }

        private InstanceExecutionStatus GetInstanceExecutionStatus(WorkflowExecutionContext context)
        {
            return context.State.ExecutionStatus switch
            {
                ExecutionStatus.Executing => InstanceExecutionStatus.Running,
                ExecutionStatus.Waiting => InstanceExecutionStatus.Waiting,
                ExecutionStatus.Completed => InstanceExecutionStatus.Completed,
                ExecutionStatus.Failed => InstanceExecutionStatus.Failed,
                _ => InstanceExecutionStatus.Pending,
            };
        }
    }
}