using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using Alma.Workflows.Monitoring.Common;
using Alma.Workflows.Monitoring.Models;
using Alma.Workflows.Monitoring.Monitors;
using Alma.Organizations.Contexts;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Alma.Workflows.Monitoring.Activities
{
    [Activity(
        Namespace = "Alma.Workflows.Monitoring",
        Category = "Monitoramento",
        DisplayName = "Média",
        Description = "Monitora a média de uma propriedade do objeto de monitoramento.")]
    [ActivityCustomization(Icon = MonitoringIcons.Monitoring, BorderColor = MonitoringColors.Default)]
    public class AverageMonitoringActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Sucesso", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Done { get; set; } = default!;

        [Port(DisplayName = "Alerta", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Warning)]
        public Port Alert { get; set; } = default!;

        [Port(DisplayName = "Falha", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Error)]
        public Port Fail { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Esquema do objeto")]
        public Parameter<string>? Schema { get; set; }

        [ActivityParameter(DisplayName = "Filtros")]
        public Parameter<List<Filter>>? Filters { get; set; }

        [ActivityParameter(DisplayName = "Campo")]
        public Parameter<string>? Field { get; set; }

        [ActivityParameter(DisplayName = "Valor")]
        public Parameter<string>? Value { get; set; }

        [ActivityParameter(DisplayName = "Intervalo")]
        public Parameter<int>? Interval { get; set; }

        [ActivityParameter(DisplayName = "Tipo de intervalo")]
        public Parameter<IntervalType>? IntervalType { get; set; }

        [ActivityParameter(DisplayName = "Tipo de desvio")]
        public Parameter<DeviationType>? DeviationType { get; set; }

        [ActivityParameter(DisplayName = "Direção do desvio")]
        public Parameter<DeviationDirection>? DeviationDirection { get; set; }

        [ActivityParameter(DisplayName = "Limite de desvio")]
        public Parameter<decimal>? DeviationLimit { get; set; }

        #endregion

        public override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var organizationContext = context.ServiceProvider.GetRequiredService<IOrganizationContext>();
            var valueMonitor = context.ServiceProvider.GetRequiredService<IValueMonitor>();

            var organzationId = await organizationContext.TryGetCurrentOrganizationId();
            var schema = Schema?.GetValue(context);
            var field = Field?.GetValue(context);

            if (string.IsNullOrWhiteSpace(organzationId))
            {
                context.State.Logs.Add("Organização não informada.", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            if (string.IsNullOrWhiteSpace(schema))
            {
                context.State.Logs.Add("Esquema do objeto não informado.", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            if (string.IsNullOrWhiteSpace(field))
            {
                context.State.Logs.Add("Campo do objeto não informado.", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            var value = Value?.GetValue(context);

            if (!Decimal.TryParse(value, CultureInfo.InvariantCulture, out var decimalValue))
            {
                context.State.Logs.Add("Valor inválido.", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            var intervalType = IntervalType?.GetValue(context) ?? Models.IntervalType.Days;
            var interval = Interval?.GetValue(context) ?? 30;
            var deviationType = DeviationType?.GetValue(context) ?? Models.DeviationType.Percentage;
            var deviationDirection = DeviationDirection?.GetValue(context) ?? Models.DeviationDirection.Both;
            var deviationLimit = DeviationLimit?.GetValue(context) ?? 100;

            var since = DateTime.UtcNow;
            since = intervalType switch
            {
                Models.IntervalType.Minutes => since.AddMinutes(-interval),
                Models.IntervalType.Hours => since.AddHours(-interval),
                Models.IntervalType.Days => since.AddDays(-interval),
                Models.IntervalType.Months => since.AddMonths(-interval),
                Models.IntervalType.Years => since.AddYears(-interval),
                _ => since.AddDays(-30),
            };

            var average = await valueMonitor.Average(organzationId, schema, field, since);

            var deviation = deviationType switch
            {
                Models.DeviationType.Percentage => (average * deviationLimit) / 100,
                Models.DeviationType.Absolute => deviationLimit,
                _ => (average * 100) / 100,
            };

            decimal? lowerLimit = deviationDirection is Models.DeviationDirection.Both or Models.DeviationDirection.Below
                ? average - deviation
                : null;

            decimal? upperLimit = deviationDirection is Models.DeviationDirection.Both or Models.DeviationDirection.Above
                ? average + deviation
                : null;

            if ((lowerLimit is not null && decimalValue < lowerLimit) || (upperLimit is not null && decimalValue > upperLimit))
            {
                context.State.Logs.Add($"Valor {decimalValue} fora do limite esperado.", Enums.LogSeverity.Warning);
                Alert.Execute();
            }
            else
            {
                context.State.Logs.Add($"Valor {decimalValue} dentro do limite esperado.", Enums.LogSeverity.Information);
                Done.Execute();
            }
        }
    }
}