using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.ApprovalsAndChecks.Attributes;
using Alma.Flows.Core.ApprovalsAndChecks.Interfaces;
using Alma.Flows.Core.Description.Descriptors;
using Alma.Core.Extensions;

namespace Alma.Flows.Core.Description.Describers
{
    public static class ApprovalAndCheckDescriber
    {
        public static ApprovalAndCheckDescriptor Describe(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            if (!type.IsAssignableTo(typeof(IApprovalAndCheck)))
                throw new Exception("Type does not implement IApprovalAndCheck.");

            var attributes = type.GetCustomAttributes(true);
            var approvalAndCheckAttribute = attributes.OfType<ApprovalAndCheckAttribute>().FirstOrDefault();

            var parameters = type.GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Parameter<>))
                .AsEnumerable();

            var descriptor = new ApprovalAndCheckDescriptor
            {
                TypeName = type.FullName ?? throw new Exception("Invalid approval and check FullName"),
                Namespace = type.Namespace?.IsNullOrEmpty(type.Namespace) ?? throw new Exception("Impossible to determine the namespace."),
                Name = type.Name,
                DisplayName = approvalAndCheckAttribute?.DisplayName.IsNullOrEmpty(type.Name) ?? throw new Exception("Impossible to determine the display name."),
                Description = approvalAndCheckAttribute?.Description,
                CanBeApprovedManually = approvalAndCheckAttribute?.CanBeApprovedManually ?? false,
                Parameters = parameters.Select(ParameterDescriber.Describe).ToList(),
                Type = type,
            };

            return descriptor;
        }
    }
}
