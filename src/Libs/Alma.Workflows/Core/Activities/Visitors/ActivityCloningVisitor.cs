using Alma.Workflows.Core.Activities.Abstractions;
using System.Text.Json;

namespace Alma.Workflows.Core.Activities.Visitors
{
    /// <summary>
    /// Result of activity cloning operation.
    /// </summary>
    public class ActivityCloneResult
    {
        public IActivity? ClonedActivity { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Visitor that creates a deep clone of an activity.
    /// Useful for copying activities in flow designers.
    /// </summary>
    public class ActivityCloningVisitor : IActivityVisitor<ActivityCloneResult>
    {
        private readonly bool _generateNewId;

        public ActivityCloningVisitor(bool generateNewId = true)
        {
            _generateNewId = generateNewId;
        }

        public ActivityCloneResult Visit(IActivity activity)
        {
            var result = new ActivityCloneResult();

            try
            {
                // Use serialization for deep cloning
                var json = JsonSerializer.Serialize(activity, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    IncludeFields = true
                });

                var clonedActivity = JsonSerializer.Deserialize(json, activity.GetType()) as IActivity;

                if (clonedActivity == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "Failed to deserialize cloned activity";
                    return result;
                }

                // Generate new ID if requested
                if (_generateNewId)
                {
                    clonedActivity.Id = Guid.NewGuid().ToString();
                }

                // Clear execution-specific state
                clonedActivity.ApprovalAndChecks.Clear();
                
                result.ClonedActivity = clonedActivity;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Cloning failed: {ex.Message}";
            }

            return result;
        }
    }
}
