using System.ComponentModel;

namespace Alma.Workflows.Core.ApprovalsAndChecks.Enums
{
    public enum ApprovalAndCheckStatus
    {
        [Description("Pendente")]
        Pending,

        [Description("Aprovado")]
        Approved,

        [Description("Rejeitado")]
        Rejected,

        [Description("Pulado")]
        Skipped,

        [Description("Cancelado")]
        Cancelled
    }
}
