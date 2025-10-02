using System.ComponentModel;

namespace Alma.Flows.Core.InstanceExecutions.Enums
{
    public enum InstanceExecutionStatus
    {
        [Description("Pendente")]
        Pending,

        [Description("Em execução")]
        Running,

        [Description("Aguardando")]
        Waiting,

        [Description("Concluído")]
        Completed,

        [Description("Falhou")]
        Failed,

        [Description("Cancelado")]
        Cancelled
    }
}
