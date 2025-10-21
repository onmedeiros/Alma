using Alma.Workflows.Builders;
using Alma.Workflows.Core.Activities.Steps;
using Alma.Workflows.Core.Description.Descriptors;
using Alma.Workflows.Definitions;
using Alma.Workflows.Registries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Parsers
{
    public interface IFlowDefinitionParser
    {
        bool TryParse(FlowDefinition definition, out Flow flow);
    }

    public class FlowDefinitionParser : IFlowDefinitionParser
    {
        private readonly ILogger<FlowDefinitionParser> _logger;
        private readonly IActivityRegistry _registry;
        private readonly ICustomActivityRegistry _customActivityRegistry;
        private readonly IApprovalAndCheckRegistry _approvalAndCheckRegistry;
        private readonly IServiceProvider _serviceProvider;

        public FlowDefinitionParser(ILogger<FlowDefinitionParser> logger, IActivityRegistry registry, IServiceProvider serviceProvider, IApprovalAndCheckRegistry approvalAndCheckRegistry, ICustomActivityRegistry customActivityRegistry)
        {
            _logger = logger;
            _registry = registry;
            _serviceProvider = serviceProvider;
            _approvalAndCheckRegistry = approvalAndCheckRegistry;
            _customActivityRegistry = customActivityRegistry;
        }

        public bool TryParse(FlowDefinition definition, out Flow flow)
        {
            var flowBuilder = _serviceProvider.GetRequiredService<IFlowBuilder>();

            // Cria a instância de todas as atividades
            foreach (var activityDefinition in definition.Activities)
            {
                ActivityDescriptor? activityDescriptor = null;

                // Search for the activity descriptor.
                if (activityDefinition.FullName.StartsWith("Alma.Workflows.Core.CustomActivities.CustomActivity"))
                {
                    activityDescriptor = _customActivityRegistry.GetActivityDescriptorAsync(activityDefinition.FullName, activityDefinition.Discriminator).GetAwaiter().GetResult();
                }
                else
                {
                    activityDescriptor = _registry.GetActivityDescriptor(activityDefinition.FullName);
                }

                if (activityDescriptor is null)
                    throw new InvalidOperationException($"Activity with fullName {activityDefinition.FullName} not registered.");

                var activityBuilder = flowBuilder.AddActivity(activityDescriptor.FullName, activityDefinition.Id, activityDefinition.Discriminator);

                // Define o nome de display da atividade
                if (string.IsNullOrEmpty(activityDefinition.CustomDisplayName))
                    activityBuilder.WithDisplayName(activityDescriptor.DisplayName);
                else
                    activityBuilder.WithDisplayName(activityDefinition.CustomDisplayName);

                // Atribui valor aos parâmetros
                // Agora os parâmetros são definidos em tempo de execução.
                // Isso permite que eles sejam atribuídos com variáveis definidas durante a execução do fluxo de trabalho.
                foreach (var activityParameter in activityDefinition.Parameters)
                {
                    var parameterDescriptor = activityDescriptor.Parameters.FirstOrDefault(x => x.Name == activityParameter.Name)
                        ?? throw new Exception($"Descriptor for parameter {activityParameter.Name} not found.");

                    activityBuilder.WithParameter(parameterDescriptor.Type, activityParameter.Name, activityParameter.ValueString);
                }

                // Adiciona o preset de etapas antes da execução da atividade
                activityBuilder
                    .WithBeforeExecutionStep<WaitConnectionsStep>()
                    .WithBeforeExecutionStep<CheckReadynessStep>()
                    .WithBeforeExecutionStep<ApprovalsStep>();

                // Adiciona o preset de etapas após a execução da atividade

                // Atribui o valor das Aprovações e Checagens
                foreach (var approvalAndCheckDefinition in activityDefinition.ApprovalAndChecks)
                {
                    var approvalAndCheckDescriptor = _approvalAndCheckRegistry.GetApprovalAndCheckDescriptor(approvalAndCheckDefinition.TypeName)
                        ?? throw new Exception($"Descriptor for approval and check {approvalAndCheckDefinition.TypeName} not found.");

                    activityBuilder.WithApprovalAndCheck(approvalAndCheckDescriptor.Type, builder =>
                    {
                        builder.WithId(approvalAndCheckDefinition.Id);

                        builder.WithParentActivity(activityBuilder.Activity);

                        if (!string.IsNullOrEmpty(approvalAndCheckDefinition.CustomName))
                            builder.WithCustomName(approvalAndCheckDefinition.CustomName);

                        foreach (var approvalAndCheckParameter in approvalAndCheckDefinition.Parameters)
                        {
                            var parameterDescriptor = approvalAndCheckDescriptor.Parameters.FirstOrDefault(x => x.Name == approvalAndCheckParameter.Name)
                                ?? throw new Exception($"Descriptor for parameter {approvalAndCheckParameter.Name} not found.");

                            builder.WithParameter(parameterDescriptor.Type, approvalAndCheckParameter.Name, approvalAndCheckParameter.ValueString);
                        }
                    });
                }
            }

            // Cria as conexões
            foreach (var connectionDefinition in definition.Connections)
            {
                flowBuilder.AddConnection(options =>
                {
                    options.WithId(connectionDefinition.Id);
                    options.WithSource(connectionDefinition.SourceActivityId, connectionDefinition.SourceActivityPort);
                    options.WithTarget(connectionDefinition.TargetActivityId, connectionDefinition.TargetActivityPort);
                });
            }

            // Retorna o Flow construído.
            flow = flowBuilder.Build();
            return true;
        }
    }
}