using System.ComponentModel;

namespace Alma.Flows.Monitoring.Models
{
    public enum DeviationType
    {
        [Description("Absoluto")]
        Absolute,

        [Description("Porcentagem")]
        Percentage
    }
}