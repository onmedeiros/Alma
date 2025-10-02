using System.ComponentModel;

namespace Alma.Flows.Models.Activities
{
    public enum VariableType
    {
        [Description("Literal")]
        String,

        [Description("Objeto JSON")]
        JsonObject
    }
}