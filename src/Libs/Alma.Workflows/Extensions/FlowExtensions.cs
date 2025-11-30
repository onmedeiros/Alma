using Alma.Workflows.Core.Activities.Abstractions;

namespace Alma.Workflows.Extensions
{
    public static class FlowExtensions
    {
        public static ICollection<IActivity> GetAllActivities(this Workflow flow)
        {
            var activities = new List<IActivity>();

            AddActivitiesRecursively(flow, activities);

            return activities;
        }

        private static void AddActivitiesRecursively(Workflow flow, ICollection<IActivity> activities)
        {
            foreach (var activity in flow.Activities)
            {
                activities.Add(activity);

                if (activity is Workflow nestedFlow)
                {
                    AddActivitiesRecursively(nestedFlow, activities);
                }
            }
        }
    }
}
