using Alma.Workflows.Definitions;

namespace Alma.Modules.Workflows.Models.Definitions
{
    public class DefinitionPublishModel
    {
        public string? Name { get; set; }
        public FlowDefinition? Definition { get; set; }
    }
}
