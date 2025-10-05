using System.ComponentModel;

namespace Alma.Flows.Core.Common.Enums
{
    public enum ParameterType
    {
        /// <summary>
        /// String parameter type.
        /// </summary>
        [Description("Literal")]
        String,

        /// <summary>
        /// Int parameter type.
        /// </summary>
        [Description("Inteiro")]
        Int,

        /// <summary>
        /// Bool parameter type.
        /// </summary>
        [Description("Booleano")]
        Bool,

        /// <summary>
        /// DateTime parameter type.
        /// </summary>
        [Description("Data e hora")]
        DateTime,

        /// <summary>
        /// Decimal parameter type.
        /// </summary>
        [Description("Decimal")]
        Decimal
    }
}