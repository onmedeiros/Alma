using Alma.Flows.Core.Activities.Attributes;
using Alma.Flows.Core.Abstractions;
using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Contexts;
using Alma.Flows.Customizations;

namespace Alma.Flows.Activities
{
    [Activity(
        Namespace = "Alma.Flows",
        Category = "Pontos de entrada",
        DisplayName = "Início",
        Description = "Esta atividade define o início do fluxo de trabalho.")]
    [ActivityCustomization(Icon = FlowIcons.Start, BorderColor = FlowColors.Entrypoints)]
    public class StartActivity : Activity, IStart
    {
        [Port(DisplayName = "Done", Type = PortType.Output, DataType = typeof(string))]
        public Port Done { get; set; } = default!;

        public override void Execute(ActivityExecutionContext context)
        {
            Done.Execute("Flow started.");
        }
    }
}