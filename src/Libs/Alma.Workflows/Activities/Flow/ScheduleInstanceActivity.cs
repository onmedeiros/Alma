using Alma.Workflows.Activities.ParameterProviders;
using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Core.InstanceExecutions.Services;
using Alma.Workflows.Customizations;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows.Activities.Flow
{
    [Activity(
        Namespace = "Alma.Workflows",
        Category = "Fluxo",
        DisplayName = "Agendar instância",
        Description = "Agenda a execução de uma instância.")]
    [ActivityCustomization(Icon = FlowIcons.Schedule, BorderColor = FlowColors.Flow)]
    public class ScheduleInstanceActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Ok", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Done { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Instância", DisplayValue = "{{value}}")]
        [ActivityParameterProvider(typeof(InstanceParameterProvider))]
        public Parameter<ParameterOption>? Instance { get; set; }

        #endregion

        public override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var instanceExecutionRunner = context.ServiceProvider.GetRequiredService<IInstanceRunner>();

            var instanceId = Instance?.GetValue(context)?.Value;

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                context.State.Logs.Add("Instância não informada.", Enums.LogSeverity.Error);
                Done.Execute();
                return;
            }

            await instanceExecutionRunner.ExecuteAsync(instanceId, null);
            Done.Execute();
        }
    }
}