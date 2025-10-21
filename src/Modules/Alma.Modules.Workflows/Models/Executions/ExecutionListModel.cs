using Alma.Workflows.Core.InstanceExecutions.Enums;

namespace Alma.Modules.Workflows.Models.Executions
{
    public record ExecutionListModel(
        string Id,
        string InstanceId,
        string DefinitionVersionId,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        string InstanceName,
        string DefinitionVersionName,
        InstanceExecutionStatus Status
    );
}
