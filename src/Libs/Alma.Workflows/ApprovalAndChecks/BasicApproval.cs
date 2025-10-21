using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.ApprovalsAndChecks.Attributes;
using Alma.Workflows.Core.ApprovalsAndChecks.Base;

namespace Alma.Workflows.ApprovalAndChecks
{
    [ApprovalAndCheck(
        Namespace = "Alma.Workflows",
        DisplayName = "Aprovação básica",
        Description = "Qualquer usuário pode aprovar manualmente a execução desta atividade.",
        CanBeApprovedManually = true)]
    public class BasicApproval : ApprovalAndCheck
    {
        #region Parameters

        [ActivityParameter(DisplayName = "Mensagem", DisplayValue = "{{value}}")]
        public Parameter<string>? Message { get; set; }

        #endregion
    }
}
