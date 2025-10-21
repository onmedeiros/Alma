using System.ComponentModel;

namespace Alma.Workflows.Monitoring.Models
{
    public enum IfExists
    {
        [Description("Lançar erro")]
        ThrowError,

        [Description("Ignorar")]
        Ignore,

        [Description("Sobrescrever")]
        Replace
    }
}