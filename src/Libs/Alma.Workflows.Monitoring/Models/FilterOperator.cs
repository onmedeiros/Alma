using System.ComponentModel;

namespace Alma.Workflows.Monitoring.Models
{
    public enum FilterOperator
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
        LessThanOrEqual,

        [Description("Contém")]
        Contains,

        [Description("Não contém")]
        NotContains
    }
}