namespace Alma.Workflows.Definitions
{
    public class FlowDefinition : ActivityDefinition
    {
        /// <summary>
        /// Start Activity Identifier.
        /// </summary>
        public string? StartId { get; set; }

        /// <summary>
        /// Activities of the flow.
        /// </summary>
        public ICollection<ActivityDefinition> Activities { get; set; } = [];

        /// <summary>
        /// Connections between activities.
        /// </summary>
        public ICollection<ConnectionDefinition> Connections { get; set; } = [];


    }
}
