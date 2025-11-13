using System.ComponentModel;

namespace Alma.Workflows.Models.Activities
{
    public enum LoopType
    {
        [Description("Contagem")]
        Count,

        [Description("Coleção")]
        Collection,

        [Description("Enquanto (While)")]
        While
    }
}
