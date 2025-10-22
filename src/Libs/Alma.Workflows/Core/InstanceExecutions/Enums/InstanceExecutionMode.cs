using System.ComponentModel;

namespace Alma.Workflows.Core.InstanceExecutions.Enums
{
    public enum InstanceExecutionMode
    {
        [Description("Automático")]
        Automatic,

        [Description("Manual")]
        Manual,

        [Description("Passo a Passo")]
        StepByStep
    }
}