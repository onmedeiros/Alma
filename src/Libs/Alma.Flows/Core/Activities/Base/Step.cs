using Alma.Flows.Core.Abstractions;
using Alma.Flows.Core.Activities.Abstractions;
using Alma.Flows.Core.Activities.Enums;
using Alma.Flows.Core.Activities.Models;
using Alma.Flows.Core.Contexts;

namespace Alma.Flows.Core.Activities.Base
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

        public ValueTask<ActivityStepData?> GetData(ActivityExecutionContext context)
        {
            var activityData = context.State.GetActivityData(context.Id);

            if (activityData.TryGetValue(Id, out var stepData) && stepData is ActivityStepData data)
            {
                return new ValueTask<ActivityStepData?>(data);
            }

            return new ValueTask<ActivityStepData?>();
        }

        public virtual ValueTask<ActivityStepStatus> ExecuteAsync(ActivityExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}