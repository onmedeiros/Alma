using Alma.Flows.Core.Activities.Attributes;
using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.ApprovalsAndChecks.Models;
using Alma.Flows.Core.Contexts;
using Alma.Flows.Customizations;

namespace Alma.Flows.Activities.Flow
{
    [Activity(
        Namespace = "Alma.Flows",
        Category = "Fluxo",
        DisplayName = "Pausa",
        Description = "Pausa a execução do fluxo de trabalho neste ponto.")]
    [ActivityCustomization(Icon = FlowIcons.Equal, BorderColor = FlowColors.Flow)]
    public class PauseActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Ok", Type = PortType.Output)]
        public Port Done { get; set; } = default!;

        #endregion

        public override IsReadyResult IsReadyToExecute(ActivityExecutionContext context)
        {
            // Aqui é onde ficarão a consulta para aprovações e checagens
            return IsReadyResult.Ready();
        }

        public override void Execute(ActivityExecutionContext context)
        {
            Done.Execute();
        }
    }
}
