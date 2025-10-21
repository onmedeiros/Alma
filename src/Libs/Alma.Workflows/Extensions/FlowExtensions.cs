using Alma.Workflows.Core.Abstractions;

namespace Alma.Workflows.Extensions
{
    public static class FlowExtensions
    {
        public static ICollection<IActivity> GetAllActivities(this Flow flow)
        {
            var activities = new List<IActivity>();

            AddActivitiesRecursively(flow, activities);

            return activities;
        }

        private static void AddActivitiesRecursively(Flow flow, ICollection<IActivity> activities)
        {
            foreach (var activity in flow.Activities)
            {
                activities.Add(activity);

                if (activity is Flow nestedFlow)
                {
                    AddActivitiesRecursively(nestedFlow, activities);
                }
            }
        }
    }
}
