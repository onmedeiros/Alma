using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.States;

namespace Alma.Workflows.Runners
{
    public class ExecutionBatchItem
    {
        public QueueItem QueueItem { get; set; }
        public ActivityRunner Runner { get; set; }
        public bool Selected { get; set; }
        public bool RequireInteraction => Runner.RequireInteraction;
        public IActivity Activity => QueueItem.Activity;

        public ExecutionBatchItem(QueueItem queueItem, ActivityRunner runner, bool selected = false)
        {
            QueueItem = queueItem;
            Runner = runner;
            Selected = selected;
        }
    }
}