using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using Alma.Workflows.Core.Activities.Abstractions;

namespace Alma.Workflows.Activities
{
    [Activity(
        Namespace = "Alma.Workflows",
        Category = "Pontos de entrada",
        DisplayName = "Início",
        Description = "Esta atividade define o início do fluxo de trabalho.")]
    [ActivityCustomization(Icon = FlowIcons.Start, BorderColor = FlowColors.Entrypoints)]
    public class StartActivity : Activity, IStart
    {
        [Port(DisplayName = "Concluído", Type = PortType.Output, DataType = typeof(string))]
        public Port Done { get; set; } = default!;

        public override void Execute(ActivityExecutionContext context)
        {
            Done.Execute("Flow started.");
        }
    }
}