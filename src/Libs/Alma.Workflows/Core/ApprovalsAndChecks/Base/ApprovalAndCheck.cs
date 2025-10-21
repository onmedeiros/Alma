using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.ApprovalsAndChecks.Interfaces;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Core.Description.Descriptors;
using Alma.Workflows.Factories;
using System.Reflection;

namespace Alma.Workflows.Core.ApprovalsAndChecks.Base
{
    public class ApprovalAndCheck : IApprovalAndCheck
    {
        public required string Id { get; set; }
        public IActivity? ParentActivity { get; set; }
        public string CustomName { get; set; } = null!;

        public ApprovalAndCheckDescriptor Descriptor { get; set; } = null!;

        public ICollection<ParameterDescriptor> ParameterDescriptors => Descriptor.Parameters;

        #region IParametrizable

        public virtual PropertyInfo GetParameterProperty(string name)
        {
            // Get all properties of type Input<>, and find the one with the specified name
            var parameterProperty = GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Parameter<>))
                .FirstOrDefault(p => p.Name == name);

            if (parameterProperty is null)
                throw new InvalidOperationException($"Parameter {name} not found.");

            return parameterProperty;
        }

        public virtual string? GetParameterValueAsString(string name)
        {
            // Get the property of type Parameter<> with the specified name
            var inputProperty = GetParameterProperty(name);

            if (inputProperty is null)
                throw new InvalidOperationException($"Parameter {name} not found.");

            var parameter = inputProperty.GetValue(this);

            if (parameter is null)
                return null;

            var valueProperty = parameter.GetType().GetProperty("Value");

            if (valueProperty is null)
                throw new InvalidOperationException($"Parameter {name} does not have a Value property.");

            var value = valueProperty.GetValue(parameter);

            return value?.ToString();
        }

        public virtual void SetParameterValue(string name, object? value)
        {
            var parameterPropertyInfo = GetParameterProperty(name);
            var parameterInstance = parameterPropertyInfo.GetValue(this);
            var valueStringPropertyInfo = parameterPropertyInfo.PropertyType.GetProperty(nameof(Parameter<object>.ValueString));

            if (valueStringPropertyInfo is null)
                throw new InvalidOperationException($"Property 'Value' not found on {parameterPropertyInfo.PropertyType}.");

            if (parameterInstance is null)
            {
                parameterInstance = ParameterFactory.CreateParameter(parameterPropertyInfo.PropertyType, value);
                parameterPropertyInfo.SetValue(this, parameterInstance);
                return;
            }

            if (value is null)
            {
                valueStringPropertyInfo.SetValue(parameterInstance, null);
                return;
            }

            valueStringPropertyInfo.SetValue(parameterInstance, value?.ToString());
        }

        #endregion

        public ValueTask<ApprovalAndCheckResult> Resolve(ActivityExecutionContext context)
        {
            return ValueTask.FromResult(ApprovalAndCheckResult.Pending);
        }

        public string GetCustomName() => CustomName;

        public void SetCustomName(string name)
        {
            CustomName = name;
        }

        public TValue? GetParameterValue<TValue>(string name, ActivityExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}