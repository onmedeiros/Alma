using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.ApprovalsAndChecks.Interfaces;
using Alma.Workflows.Factories;
using System.Reflection;

namespace Alma.Workflows.Extensions
{
    public static class ApprovalAndCheckExtensions
    {
        public static PropertyInfo? GetParameterPropertyInfo(this IApprovalAndCheck approvalAndCheck, string name)
        {
            // Get all properties of type Input<>, and find the one with the specified name
            var parameterPropertyInfo = approvalAndCheck.GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Parameter<>))
                .FirstOrDefault(p => p.Name == name);

            return parameterPropertyInfo;
        }

        public static void SetParameterValue(this IApprovalAndCheck approvalAndCheck, Type type, string name, object? value)
        {
            var parameterPropertyInfo = approvalAndCheck.GetParameterPropertyInfo(name);

            if (parameterPropertyInfo is null)
                throw new InvalidOperationException($"Parameter {name} not found.");

            var genericType = parameterPropertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();

            if (genericType is null || !genericType.IsAssignableFrom(type))
                throw new InvalidOperationException($"Couldn't set parameter. Invalid data type, expected {genericType}, but got {type}.");

            try
            {
                var parameter = ParameterFactory.CreateParameter(type, value);
                parameterPropertyInfo.SetValue(approvalAndCheck, parameter);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set parameter {name}.", ex);
            }
        }
    }
}
