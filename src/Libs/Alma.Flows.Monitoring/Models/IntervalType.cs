using System.ComponentModel;

namespace Alma.Flows.Monitoring.Models
{
    public enum IntervalType
    {
        [Description("Segundos")]
        Seconds,

        [Description("Minutos")]
        Minutes,

        [Description("Horas")]
        Hours,

        [Description("Dias")]
        Days,

        [Description("Semanas")]
        Months,

        [Description("Anos")]
        Years
    }
}