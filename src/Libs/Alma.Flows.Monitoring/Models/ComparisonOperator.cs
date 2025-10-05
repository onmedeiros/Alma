using System.ComponentModel;

namespace Alma.Flows.Monitoring.Models
{
    public enum ComparisonOperator
    {
        [Description("==")]
        Equals,

        [Description("!=")]
        NotEquals,

        [Description(">")]
        GreaterThan,

        [Description(">=")]
        GreaterThanOrEqual,

        [Description("<")]
        LessThan,

        [Description("<=")]
        LessThanOrEqual
    }
}