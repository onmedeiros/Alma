using Alma.Flows.Core.Activities.Attributes;
using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Contexts;
using Alma.Flows.Customizations;

namespace Alma.Flows.Activities.Interaction
{
    [Activity(
        Namespace = "Alma.Flows",
        Category = "Interação",
        DisplayName = "Instrução",
        Description = "Exibe uma instrução para o usuário.",
        RequireInteraction = true)]
    [ActivityCustomization(Icon = FlowIcons.Instruction, BorderColor = FlowColors.Interaction)]
    public class InstructionActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Ok", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Done { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Texto", AutoGrow = true, Lines = 4, MaxLines = 8)]
        public Parameter<string>? Text { get; set; }

        #endregion

        public override ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            Done.Execute();
            return ValueTask.CompletedTask;
        }
    }
}