using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using Alma.Workflows.Enums;
using Alma.Workflows.Models.Activities;

namespace Alma.Workflows.Activities.Interaction
{
    [Activity(
        Namespace = "Alma.Workflows",
        Category = "Interação",
        DisplayName = "Formulário",
        Description = "Exibe um formulário para interação com o usuário.",
        RequireInteraction = true)]
    [ActivityCustomization(Icon = FlowIcons.Form, BorderColor = FlowColors.Interaction)]
    public class FormActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Ok", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Done { get; set; } = default!;

        #endregion Ports

        #region Parameters

        [ActivityParameter(DisplayName = "Título", DisplayValue = "{{value}}")]
        public Parameter<string>? Title { get; set; }

        [ActivityParameter(DisplayName = "Campos")]
        public Parameter<ICollection<FormField>>? Fields { get; set; }

        #endregion Parameters

        #region Data

        public Data<Dictionary<string, object?>>? FormState { get; set; }

        #endregion Data

        public override IsReadyResult IsReadyToExecute(ActivityExecutionContext context)
        {
            // Validate fields
            if (Fields?.GetValue(context) == null || Fields.GetValue(context).Count == 0)
            {
                return IsReadyResult.Ready();
            }

            foreach (var field in Fields.GetValue(context))
            {
                // Validate required field
                if (field.Required)
                {
                    var value = FormState?.Value?.GetValueOrDefault(field.Name);

                    // Valida valor nulo
                    if (value is null)
                        return IsReadyResult.NotReady($"O campo '{field.Label}' é obrigatório.");

                    // Valida valores string
                    if (field.Type == FieldType.Text && string.IsNullOrEmpty(value?.ToString()))
                        return IsReadyResult.NotReady($"O campo '{field.Label}' é obrigatório.");
                }
            }

            return IsReadyResult.Ready();
        }

        public override void Execute(ActivityExecutionContext context)
        {
            Done.Execute(FormState?.Value);
        }
    }
}