using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using NCalc;

namespace Alma.Workflows.Activities.Flow
{
    [Activity(
        Namespace = "Alma.Workflows",
        Category = "Fluxo",
        DisplayName = "Condição",
        Description = "Executa uma expressão lógica.")]
    [ActivityCustomization(Icon = FlowIcons.Equal, BorderColor = FlowColors.Flow)]
    public class ConditionActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Verdadeiro", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port True { get; set; } = default!;

        [Port(DisplayName = "Falso", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Error)]
        public Port False { get; set; } = default!;

        [Port(DisplayName = "Erro", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Error)]
        public Port Fail { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Expressão lógica", DisplayValue = "{{value}}")]
        public Parameter<string>? Expression { get; set; }

        #endregion

        public override void Execute(ActivityExecutionContext context)
        {
            var expression = new Expression(Expression?.GetValue(context));

            // Verifica se a expressão não contém erros
            if (expression.HasErrors())
            {
                context.State.Logs.Add($"A expressão '{Expression?.GetValue(context)}' é invalida: {expression.Error.Message}", Enums.LogSeverity.Error);
                Fail.Execute($"A expressão '{Expression?.GetValue(context)}' é invalida.");
                return;
            }

            var result = expression.Evaluate();

            if (result is bool booleanResult)
            {
                if (booleanResult)
                {
                    True.Execute();
                    return;
                }
                else
                {
                    False.Execute();
                    return;
                }
            }
            else
            {
                context.State.Logs.Add($"A expressão '{Expression?.GetValue(context)}' não é uma expressão lógica.", Enums.LogSeverity.Error);
                Fail.Execute($"A expressão '{Expression?.GetValue(context)}' não é uma expressão lógica.");
            }
        }
    }
}