using System.ComponentModel;

namespace Alma.Workflows.Monitoring.Models
{
    public enum DeviationType
    {
        [Description("Absoluto")]
        Absolute,

        [Description("Porcentagem")]
        Percentage
    }
}