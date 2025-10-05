using Alma.Flows.Core.Activities.Attributes;
using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Customizations;
using Alma.Flows.Monitoring.Common;
using Alma.Flows.Monitoring.Models;

namespace Alma.Flows.Monitoring.Activities
{
    [Activity(
        Namespace = "Alma.Flows.Monitoring",
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
    }
}