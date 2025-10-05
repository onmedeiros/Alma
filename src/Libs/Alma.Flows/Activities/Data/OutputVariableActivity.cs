using Alma.Flows.Core.Activities.Attributes;
using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Contexts;
using Alma.Flows.Customizations;

namespace Alma.Flows.Activities.Data
{
    [Activity(
        Namespace = "Alma.Flows",
        Category = "Dados",
        DisplayName = "Variável de saída",
        Description = "O valor desta variável será a saída da atividade conectada.")]
    [ActivityCustomization(Icon = FlowIcons.Variable, BorderColor = FlowColors.Data)]
    public class OutputVariableActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Ok", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Done { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Nome", DisplayValue = "<b>Nome:</b> {{value}}")]
        public Parameter<string>? Name { get; set; }

        #endregion

        public override void Execute(ActivityExecutionContext context)
        {
            var lastExecution = context.State.ExecutedConnections.LastOrDefault(x => x.TargetId == Id && x.TargetPortName == nameof(Input));

            if (!string.IsNullOrEmpty(Name?.GetValue(context)))
                context.State.SetVariable(Name.GetValue(context), lastExecution?.Data?.Value);

            Done.Execute(lastExecution?.Data);
        }
    }
}