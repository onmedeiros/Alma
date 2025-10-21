﻿using Alma.Workflows.States;

namespace Alma.Workflows.Runners
{
    public class FlowExecution
    {
        public QueueItem QueueItem { get; set; }
        public ActivityRunner Runner { get; set; }
        public bool Selected { get; set; }
        public bool RequireInteraction => Runner.RequireInteraction;

        public FlowExecution(QueueItem queueItem, ActivityRunner runner, bool selected = false)
        {
            QueueItem = queueItem;
            Runner = runner;
            Selected = selected;
        }
    }
}