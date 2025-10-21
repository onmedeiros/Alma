using Alma.Workflows.Core.Abstractions;

namespace Alma.Workflows.Options
{
    public class FlowOptions
    {
        private ICollection<Type> _activityTypes = [];
        private ICollection<Type> _approvalAndChecksTypes = [];

        public IEnumerable<Type> ActivityTypes => _activityTypes;
        public IEnumerable<Type> ApprovalAndChecksTypes => _approvalAndChecksTypes;

        public FlowOptions AddActivity<TActivity>()
            where TActivity : IActivity
        {
            _activityTypes.Add(typeof(TActivity));
            return this;
        }

        public FlowOptions AddApprovalAndCheck<TApprovalAndCheck>()
        {
            _approvalAndChecksTypes.Add(typeof(TApprovalAndCheck));
            return this;
        }
    }
}