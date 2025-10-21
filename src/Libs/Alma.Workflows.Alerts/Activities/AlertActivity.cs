using Alma.Organizations.Contexts;
using Alma.Workflows.Alerts.Common;
using Alma.Workflows.Alerts.Services;
using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows.Alerts.Activities
{
    [Activity(
        Namespace = "Alma.Workflows.Alerts",
        Category = "Notificações",
        DisplayName = "Alerta",
        Description = "Registra um alerta com as informações configuradas.")]
    [ActivityCustomization(Icon = Icons.Alert, BorderColor = Colors.Default)]
    public class AlertActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Ok", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Done { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Severidade", DisplayValue = "{{value}}")]
        public Parameter<AlertSeverity>? Severity { get; set; }

        [ActivityParameter(DisplayName = "Título", DisplayValue = "{{value}}")]
        public Parameter<string>? Title { get; set; }

        [ActivityParameter(DisplayName = "Detalhes", AutoGrow = true, Lines = 3, MaxLines = 8)]
        public Parameter<string>? Details { get; set; }

        #endregion

        public override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var severity = Severity?.GetValue(context) ?? AlertSeverity.Low;
            var title = Title?.GetValue(context) ?? "Alerta";
            var details = Details?.GetValue(context);

            var organizationContext = context.ServiceProvider.GetRequiredService<IOrganizationContext>();
            var alertService = context.ServiceProvider.GetRequiredService<IAlertService>();

            var organizationId = await organizationContext.TryGetCurrentOrganizationId();
            var result = await alertService.Create(severity, title, details, organizationId);

            if (!result.Succeeded)
            {
                context.State.Log($"Failed to create alert: {string.Join(',', result.Errors?.Select(e => e.Message) ?? [])}");
                throw new InvalidOperationException("Failed to create alert.");
            }

            Done.Execute();
        }
    }
}