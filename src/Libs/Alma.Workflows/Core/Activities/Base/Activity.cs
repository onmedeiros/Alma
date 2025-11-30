using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.ApprovalsAndChecks.Enums;
using Alma.Workflows.Core.ApprovalsAndChecks.Interfaces;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Core.Description.Descriptors;
using Alma.Workflows.Core.Properties;
using System.Reflection;

namespace Alma.Workflows.Core.Activities.Base
{
    /// <summary>
    /// Base class for all activities in a workflow.
    /// Refactored to use Property Accessors for better performance and maintainability.
    /// </summary>
    public class Activity : IActivity
    {
        private ApprovalAndCheckStatus _approvalAndCheckStatus = ApprovalAndCheckStatus.Pending;

        // Property Accessors - lazy initialized for performance
        private static readonly Lazy<PropertyAccessorFactory> _accessorFactory =
            new Lazy<PropertyAccessorFactory>(() => PropertyAccessorFactory.Create());

        protected static PropertyAccessorFactory Accessors => _accessorFactory.Value;

        public string Id { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public ActivityDescriptor Descriptor { get; private set; } = null!;

        public ApprovalAndCheckStatus ApprovalAndCheckStatus => _approvalAndCheckStatus;

        public ICollection<IApprovalAndCheck> ApprovalAndChecks { get; set; } = [];

        public IList<IStep> BeforeExecutionSteps { get; set; } = [];
        public IList<IStep> AfterExecutionSteps { get; set; } = [];
        public ICollection<ParameterDescriptor> ParameterDescriptors => Descriptor.Parameters;

        public Activity()
        {
            Id = Guid.NewGuid().ToString();
        }

        public Activity(string? id)
        {
            if (!string.IsNullOrEmpty(id))
                Id = id;
            else
                Id = Guid.NewGuid().ToString();
        }

        #region IParametrizable - Using Property Accessors for Performance

        public virtual PropertyInfo GetParameterProperty(string name)
        {
            var propertyInfo = Accessors.Parameters.GetPropertyInfo(this, name);

            if (propertyInfo is null)
                throw new InvalidOperationException($"Parameter {name} not found.");

            return propertyInfo;
        }

        public virtual TValue? GetParameterValue<TValue>(string name, ActivityExecutionContext context)
        {
            var propertyInfo = Accessors.Parameters.GetPropertyInfo(this, name);

            if (propertyInfo is null)
                throw new InvalidOperationException($"Parameter {name} not found.");

            var parameter = (Parameter<TValue>?)propertyInfo.GetValue(this);

            if (parameter is null)
                return default;

            return parameter.GetValue(context);
        }

        public virtual string? GetParameterValueAsString(string name)
        {
            return Accessors.Parameters.GetValueAsString(this, name);
        }

        public virtual void SetParameterValue(string name, object? value)
        {
            Accessors.Parameters.Set(this, name, value);
        }

        #endregion

        #region IConnectable - Using Property Accessors for Performance

        public virtual IEnumerable<PropertyInfo> GetPortProperties()
        {
            return Accessors.Ports.GetPropertyInfos(this);
        }

        public virtual PropertyInfo GetPortProperty(string name)
        {
            var propertyInfo = Accessors.Ports.GetPropertyInfo(this, name);

            if (propertyInfo is null)
                throw new InvalidOperationException($"Port property with name '{name}' doesn't exist.");

            return propertyInfo;
        }

        public virtual void SetPortProperty(string name, Port value)
        {
            Accessors.Ports.Set(this, name, value);
        }

        public virtual IEnumerable<Port> GetPorts()
        {
            return Accessors.Ports.GetPorts(this);
        }

        public virtual void SetPortData(string name, object? value)
        {
            Accessors.Ports.SetPortData(this, name, value);
        }

        #endregion

        public virtual void Execute(ActivityExecutionContext context)
        {
            return;
        }

        public virtual ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            Execute(context);
            return ValueTask.CompletedTask;
        }

        #region Is Ready

        public virtual IsReadyResult IsReadyToExecute(ActivityExecutionContext context)
        {
            return IsReadyResult.Ready();
        }

        public virtual ValueTask<IsReadyResult> IsReadyToExecuteAsync(ActivityExecutionContext context)
        {
            return ValueTask.FromResult(IsReadyToExecute(context));
        }

        #endregion

        public void SetDescriptor(ActivityDescriptor descriptor)
        {
            Descriptor = descriptor;
        }
    }
}