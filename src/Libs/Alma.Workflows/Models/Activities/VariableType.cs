using System.ComponentModel;

namespace Alma.Workflows.Models.Activities
{
    public enum VariableType
    {
        [Description("Literal")]
        String,

        [Description("Objeto JSON")]
        JsonObject
    }
}