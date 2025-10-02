using Alma.Flows.Core.Activities.Attributes;
using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.ApprovalsAndChecks.Attributes;
using Alma.Flows.Core.ApprovalsAndChecks.Base;

namespace Alma.Flows.ApprovalAndChecks
{
    [ApprovalAndCheck(
        Namespace = "Alma.Flows",
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
