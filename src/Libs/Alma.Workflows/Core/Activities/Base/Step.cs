using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.Activities.Enums;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.Contexts;

namespace Alma.Workflows.Core.Activities.Base
{
    public class Step : IStep
    {
        private IActivity? _activity;

        public string Id { get; private set; } = Guid.NewGuid().ToString();

        public IActivity Activity => _activity ?? throw new InvalidOperationException("Activity is not set.");

        public void SetId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id cannot be null or whitespace.", nameof(id));

            Id = id;
        }

        public void SetActivity(IActivity activity)
        {
            if (_activity is not null)
                throw new InvalidOperationException("Activity is already set.");

            _activity = activity;
        }

        public async ValueTask<ActivityStepStatus> GetStatus(ActivityExecutionContext context)
        {
            var data = await GetData(context);

            if (data is null)
                return ActivityStepStatus.Pending;

            return data.Status;
        }

        public ValueTask<ActivityStepData> GetData(ActivityExecutionContext context)
        {
            var stepData = context.State.Steps.GetStepData(Activity.Id, Id);

            if (stepData is null)
            {
                stepData = new ActivityStepData
                {
                    Id = Id,
                    ActivityId = Activity.Id,
                    Status = ActivityStepStatus.Pending
                };

                context.State.Steps.SetStepData(stepData);
            }

            return ValueTask.FromResult(stepData);
        }

        public virtual ValueTask<ActivityStepStatus> ExecuteAsync(ActivityExecutionContext context)
        {
            // This is a base implementation that should be overridden by derived classes.
            throw new NotImplementedException();
        }
    }
}