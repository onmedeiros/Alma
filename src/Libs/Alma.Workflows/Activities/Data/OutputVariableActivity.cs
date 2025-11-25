using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;

namespace Alma.Workflows.Activities.Data
{
    [Activity(
        Namespace = "Alma.Workflows",
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
            var lastExecution = context.State.Connections.AsCollection().LastOrDefault(x => x.TargetId == Id && x.TargetPortName == nameof(Input));

            if (!string.IsNullOrEmpty(Name?.GetValue(context)))
                context.State.Variables.Set(Name.GetValue(context)!, lastExecution?.Data?.GetValue());

            Done.Execute(lastExecution?.Data);
        }
    }
}