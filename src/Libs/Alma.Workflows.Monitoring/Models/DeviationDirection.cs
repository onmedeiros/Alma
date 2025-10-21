using System.ComponentModel;

namespace Alma.Workflows.Monitoring.Models
{
    public enum DeviationDirection
    {
        [Description("Acima")]
        Above,

        [Description("Abaixo")]
        Below,

        [Description("Ambos")]
        Both
    }
}