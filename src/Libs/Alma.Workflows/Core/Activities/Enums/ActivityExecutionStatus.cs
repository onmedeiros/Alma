using System.ComponentModel;

namespace Alma.Workflows.Enums
{
    public enum ActivityExecutionStatus
    {
        /// <summary>
        /// Aguardando que as atividades anteriores seja finalizadas para executar.
        /// </summary>
        [Description("Pendente")]
        Pending,

        /// <summary>
        /// Aguardando execução das etapas.
        /// </summary>
        [Description("Aguardando")]
        Waiting,

        /// <summary>
        /// Aguardando aprovações e checagens.
        /// </summary>
        [Description("Aguardando Aprovações")]
        WaitingApprovalAndChecks,

        /// <summary>
        /// Pronta para execução
        /// </summary>
        [Description("Pronta")]
        Ready,

        /// <summary>
        /// Execução completada.
        /// </summary>
        [Description("Completada")]
        Completed,

        /// <summary>
        /// Falha na execução
        /// </summary>
        [Description("Falha")]
        Failed
    }
}