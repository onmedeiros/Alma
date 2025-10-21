using System.ComponentModel;

namespace Alma.Workflows.Alerts.Common
{
    public enum AlertType
    {
        [Description("Informação")]
        Information,

        [Description("Alerta")]
        Warning,

        [Description("Erro")]
        Error
    }
}