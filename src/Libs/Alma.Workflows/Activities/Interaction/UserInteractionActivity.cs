using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;

namespace Alma.Workflows.Activities.Interaction
{
    [Activity(
        Namespace = "Alma.Workflows",
        Category = "Interação",
        DisplayName = "Interação com o Usuário",
        Description = "Espera uma interação com o usuário para continuar.",
        RequireInteraction = true)]
    [ActivityCustomization(Icon = FlowIcons.Person, BorderColor = FlowColors.Interaction)]
    public class UserInteractionActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Ok", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Done { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Conteúdo", DisplayValue = "{{value}}")]
        public Parameter<string>? Content { get; set; }

        [ActivityParameter(DisplayName = "Ações")]
        public Parameter<Dictionary<string, string>>? Actions { get; set; }

        #endregion

        public override ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            Done.Execute();
            return ValueTask.CompletedTask;
        }
    }
}