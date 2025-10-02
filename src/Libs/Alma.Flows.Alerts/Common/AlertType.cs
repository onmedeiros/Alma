using System.ComponentModel;

namespace Alma.Flows.Alerts.Common
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