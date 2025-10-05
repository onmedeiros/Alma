using System.ComponentModel;

namespace Alma.Flows.Monitoring.Models
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