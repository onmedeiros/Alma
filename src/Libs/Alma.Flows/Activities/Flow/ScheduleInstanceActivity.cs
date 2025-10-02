using Alma.Flows.Activities.ParameterProviders;
using Alma.Flows.Core.Activities.Attributes;
using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Activities.Models;
using Alma.Flows.Core.Contexts;
using Alma.Flows.Core.InstanceExecutions.Services;
using Alma.Flows.Customizations;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Flows.Activities.Flow
{
    [Activity(
        Namespace = "Alma.Flows",
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
            var instanceExecutionRunner = context.ServiceProvider.GetRequiredService<IInstanceExecutionRunner>();

            var instanceId = Instance?.GetValue(context)?.Value;

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                context.State.Log("Instância não informada.", Enums.LogSeverity.Error);
                Done.Execute();
                return;
            }

            await instanceExecutionRunner.ExecuteAsync(instanceId, null);
            Done.Execute();
        }
    }
}