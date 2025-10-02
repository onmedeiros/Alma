using Alma.Flows.Core.Activities.Attributes;
using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Contexts;
using Alma.Flows.Customizations;
using Alma.Flows.Models.Activities;
using Alma.Flows.Utils;

namespace Alma.Flows.Activities.Data
{
    [Activity(
        Namespace = "Alma.Flows",
        Category = "Dados",
        DisplayName = "Variável",
        Description = "Cria uma nova variável, ou atribui valor a um variável existente, no contexto de execução.")]
    [ActivityCustomization(Icon = FlowIcons.Variable, BorderColor = FlowColors.Data)]
    public class VariableActivity : Activity
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

        [ActivityParameter(DisplayName = "Valor", DisplayValue = "<b>Valor:</b> {{value}}", AutoGrow = true, MaxLines = 8)]
        public Parameter<string>? Value { get; set; }

        [ActivityParameter(DisplayName = "Tipo", DisplayValue = "<b>Tipo:</b> {{value}}", AutoGrow = true, MaxLines = 8)]
        public Parameter<VariableType>? Type { get; set; }

        #endregion

        public override void Execute(ActivityExecutionContext context)
        {
            var name = Name?.GetValue(context);
            var value = Value?.GetValue(context);
            var type = Type?.GetValue(context) ?? VariableType.String;

            if (string.IsNullOrEmpty(name))
            {
                Done.Execute();
                return;
            }

            if (type == VariableType.String)
            {
                context.State.SetVariable(name, value);
                Done.Execute();
            }
            else if (type == VariableType.JsonObject)
            {
                var obj = DotLiquidUtils.ConvertJsonToHash(value ?? "{}");
                context.State.SetVariable(name, obj);
                Done.Execute();
            }
            else
            {
                context.State.SetVariable(name, value);
                Done.Execute();
            }
        }
    }
}