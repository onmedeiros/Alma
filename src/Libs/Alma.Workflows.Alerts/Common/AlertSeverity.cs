using System.ComponentModel;

namespace Alma.Workflows.Alerts.Common
{
    public enum AlertSeverity
    {
        [Description("Baixa")]
        Low,

        [Description("Média")]
        Medium,

        [Description("Alta")]
        High,

        [Description("Crítica")]
        Critical
    }
}