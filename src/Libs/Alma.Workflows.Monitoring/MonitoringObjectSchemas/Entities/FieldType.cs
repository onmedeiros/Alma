using System.ComponentModel;

namespace Alma.Workflows.Monitoring.MonitoringObjectSchemas.Entities
{
    public enum FieldType
    {
        [Description("Literal")]
        String,

        [Description("Inteiro")]
        Integer,

        [Description("Decimal")]
        Decimal,

        [Description("Booleano")]
        Boolean,

        [Description("Data e hora")]
        DateTime
    }
}