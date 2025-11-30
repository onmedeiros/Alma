using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.ApprovalsAndChecks.Interfaces;
using Alma.Workflows.Core.Description.Descriptors;
using Alma.Workflows.Extensions;

namespace Alma.Workflows.Builders
{
    public class ApprovalAndCheckBuilder
    {
        private ApprovalAndCheckDescriptor _descriptor;
        private IApprovalAndCheck _instance;

        public IApprovalAndCheck Instance => _instance;

        public ApprovalAndCheckBuilder(ApprovalAndCheckDescriptor descriptor)
        {
            _descriptor = descriptor;

            if (Activator.CreateInstance(descriptor.Type) is not IApprovalAndCheck instance)
                throw new InvalidOperationException($"Type {descriptor.Type} does not implement {nameof(IApprovalAndCheck)}");

            _instance = instance;
            _instance.Descriptor = descriptor;
        }

        public ApprovalAndCheckBuilder WithId(string id)
        {
            _instance.Id = id;

            return this;
        }

        public ApprovalAndCheckBuilder WithCustomName(string name)
        {
            _instance.SetCustomName(name);
            return this;
        }

        public ApprovalAndCheckBuilder WithParentActivity(IActivity activity)
        {
            _instance.ParentActivity = activity;
            return this;
        }

        public ApprovalAndCheckBuilder WithParameter(Type type, string name, object? value)
        {
            _instance.SetParameterValue(type, name, value);

            return this;
        }


    }
}
