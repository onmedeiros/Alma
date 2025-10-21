using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using NCalc;
using Alma.Core.Extensions;
using System.Globalization;

namespace Alma.Workflows.Activities.Data
{
    [Activity(
        Namespace = "Alma.Workflows",
        Category = "Dados",
        DisplayName = "Calcular",
        Description = "Executa uma expressão de cálculo.")]
    [ActivityCustomization(Icon = FlowIcons.Calculate, BorderColor = FlowColors.Data)]
    public class CalculateActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Ok", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Done { get; set; } = default!;

        [Port(DisplayName = "Erro", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Error)]
        public Port Fail { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Expressão", DisplayValue = "{{value}}")]
        public Parameter<string>? Expression { get; set; }

        #endregion

        public override void Execute(ActivityExecutionContext context)
        {
            // Valida se a expressão não é uma expressão lógica
            if (Expression?.GetValue(context)?.ContainsLogicalOperator() == true)
            {
                Fail.Execute($"A expressão '{Expression?.GetValue(context)}' é invalida. Esta atividade não permite operações lógicas.");
                return;
            }

            var expression = new Expression(Expression?.GetValue(context));
            expression.CultureInfo = CultureInfo.InvariantCulture;

            // Verifica se a expressão não contém erros
            if (expression.HasErrors())
            {
                Fail.Execute($"A expressão '{Expression?.GetValue(context)}' é invalida.");
                return;
            }

            try
            {
                var result = expression.Evaluate();
                var convertedResult = Convert.ToDecimal(result);
                Done.Execute(convertedResult);
            }
            catch
            {
                Fail.Execute($"A expressão '{Expression?.GetValue(context)}' não retornou um valor numérico.");
            }
        }
    }
}