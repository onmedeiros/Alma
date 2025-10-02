using Alma.Flows.Core.Abstractions;
using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Activities.Factories;
using Alma.Flows.Registries;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Alma.Flows.Builders
{
    public interface IActivityBuilder<TActivity> : IActivityBuilder
        where TActivity : class, IActivity
    {
        ActivityBuilder<TActivity> Begin(string? id = null);

        ActivityBuilder<TActivity> WithParameter<TValue>(string name, TValue value);

        public ActivityBuilder<TActivity> WithParameter<TValue>(Expression<Func<TActivity, Parameter<TValue>?>> parameterExpression, TValue value);

        public new TActivity Build();
    }

    public class ActivityBuilder<TActivity> : ActivityBuilder, IActivityBuilder<TActivity>
        where TActivity : class, IActivity
    {
        public ActivityBuilder(ILogger<ActivityBuilder> logger, IActivityRegistry activityRegistry, IApprovalAndCheckRegistry approvalAndCheckRegistry, ICustomActivityRegistry customActivityRegistry, IActivityStepFactory activityStepFactory) : base(logger, activityRegistry, approvalAndCheckRegistry, customActivityRegistry, activityStepFactory)
        {
        }

        public ActivityBuilder<TActivity> Begin(string? id = null)
        {
            var descriptor = ActivityRegistry.GetActivityDescriptor(typeof(TActivity));

            base.Begin(descriptor, id);

            return this;
        }

        public ActivityBuilder<TActivity> WithParameter<TValue>(string name, TValue value)
        {
            Activity!.SetParameterValue(name, value);

            return this;
        }

        public ActivityBuilder<TActivity> WithParameter<TValue>(Expression<Func<TActivity, Parameter<TValue>?>> parameterExpression, TValue value)
        {
            if (parameterExpression.Body is MemberExpression memberExpression)
            {
                var parameterName = memberExpression.Member.Name;
                Activity!.SetParameterValue(parameterName, value);
            }
            else
            {
                throw new ArgumentException("Invalid parameter expression", nameof(parameterExpression));
            }

            return this;
        }

        public override TActivity Build()
        {
            return Activity as TActivity ??
                throw new Exception($"Impossible to build activity of type {typeof(TActivity)}.");
        }
    }
}