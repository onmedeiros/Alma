using System.ComponentModel;

namespace Alma.Flows.Enums
{
    public enum LogSeverity
    {
        [Description("Informação")]
        Information,

        [Description("Aviso")]
        Warning,

        [Description("Erro")]
        Error,

        [Description("Crítico")]
        Critical,

        [Description("Depuração")]
        Debug
    }
}
