using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using Alma.Workflows.Monitoring.Common;
using Alma.Workflows.Monitoring.Models;
using Alma.Workflows.Monitoring.Monitors;
using Alma.Organizations.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows.Monitoring.Activities
{
    [Activity(
        Namespace = "Alma.Workflows.Monitoring",
        Category = "Monitoramento",
        DisplayName = "Contagem",
        Description = "Monitora a contagem de um objeto de monitoramento.")]
    [ActivityCustomization(Icon = MonitoringIcons.Monitoring, BorderColor = MonitoringColors.Default)]
    public class CountMonitoringActivity : Activity
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

        [ActivityParameter(DisplayName = "Intervalo")]
        public Parameter<int>? Interval { get; set; }

        [ActivityParameter(DisplayName = "Tipo de intervalo")]
        public Parameter<IntervalType>? IntervalType { get; set; }

        [ActivityParameter(DisplayName = "Operador")]
        public Parameter<ComparisonOperator>? Operator { get; set; }

        [ActivityParameter(DisplayName = "Contagem")]
        public Parameter<int>? Count { get; set; }

        #endregion

        public override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var organizationContext = context.ServiceProvider.GetRequiredService<IOrganizationContext>();
            var monitoringObjectMonitor = context.ServiceProvider.GetRequiredService<IMonitoringObjectMonitor>();

            var organzationId = await organizationContext.TryGetCurrentOrganizationId();
            var schema = Schema?.GetValue(context);
            var filters = Filters?.GetValue(context) ?? [];

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

            var intervalType = IntervalType?.GetValue(context) ?? Models.IntervalType.Days;
            var interval = Interval?.GetValue(context) ?? 30;
            var comparisonOperator = Operator?.GetValue(context) ?? ComparisonOperator.GreaterThan;
            var count = Count?.GetValue(context) ?? 100;

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

            // Execute the monitoring object count
            var result = await monitoringObjectMonitor.Count(organzationId, schema, filters, since);

            context.State.Logs.Add($"Contagem de objetos de monitoramento para o esquema '{schema}' desde {since} é {result}.", Enums.LogSeverity.Information);

            // Compare the result with the threshold
            var shouldAlert = comparisonOperator switch
            {
                ComparisonOperator.GreaterThan => result > count,
                ComparisonOperator.GreaterThanOrEqual => result >= count,
                ComparisonOperator.LessThan => result < count,
                ComparisonOperator.LessThanOrEqual => result <= count,
                ComparisonOperator.Equals => result == count,
                ComparisonOperator.NotEquals => result != count,
                _ => false,
            };

            if (shouldAlert)
            {
                context.State.Logs.Add($"Valor de contagem ({result}) fora do limite esperado.", Enums.LogSeverity.Warning);
                Alert.Execute();
            }
            else
            {
                context.State.Logs.Add("Valor de contagem dentro do limite esperado.", Enums.LogSeverity.Information);
                Done.Execute();
            }
        }
    }
}