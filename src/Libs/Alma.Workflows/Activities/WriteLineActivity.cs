using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using Alma.Workflows.Enums;

namespace Alma.Workflows.Activities
{
    [Activity(
        Namespace = "Alma.Workflows",
        Category = "Base",
        DisplayName = "Log",
        Description = "Esta atividade adiciona uma mensagem no log de execução.")]
    [ActivityCustomization(Icon = FlowIcons.Terminal, BorderColor = FlowColors.Base)]
    public class WriteLineActivity : Activity
    {
        [ActivityParameter(DisplayName = "Severidade")]
        public Parameter<LogSeverity>? Severity { get; set; }

        [ActivityParameter(DisplayName = "Mensagem", DisplayValue = "{{value}}")]
        public Parameter<string>? Message { get; set; }

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Ok", Type = PortType.Output)]
        public Port Done { get; set; } = default!;

        public override void Execute(ActivityExecutionContext context)
        {
            context.State.Log(Message?.GetValue(context), Severity?.GetValue(context));

            Done.Execute();
        }
    }
}