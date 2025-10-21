using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.ApprovalsAndChecks.Enums;
using Alma.Workflows.Core.ApprovalsAndChecks.Interfaces;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Core.Description.Descriptors;
using Alma.Workflows.Factories;
using System.Reflection;

namespace Alma.Workflows.Core.Activities.Base
{
    public class Activity : IActivity
    {
        private ApprovalAndCheckStatus _approvalAndCheckStatus = ApprovalAndCheckStatus.Pending;

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

        public virtual TValue? GetParameterValue<TValue>(string name, ActivityExecutionContext context)
        {
            // Get the property of type Input<> with the specified name
            var inputProperty = GetParameterProperty(name);

            if (inputProperty is null)
                throw new InvalidOperationException($"Parameter {name} not found.");

            var parameter = (Parameter<TValue>?)inputProperty.GetValue(this);

            if (parameter is null)
                return default;

            return parameter.GetValue(context);
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
                throw new InvalidOperationException($"Property 'ValueString' not found on {parameterPropertyInfo.PropertyType}.");

            if (parameterInstance is null)
            {
                var parameterGenericType = parameterPropertyInfo.PropertyType.GenericTypeArguments[0];
                parameterInstance = ParameterFactory.CreateParameter(parameterGenericType, value);
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

        #region IDataContaining

        public virtual PropertyInfo GetDataProperty(string name)
        {
            // Get all properties of type Data<>, and find the one with the specified name
            var dataProperty = GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Data<>))
                .FirstOrDefault(p => p.Name == name);

            if (dataProperty is null)
                throw new InvalidOperationException($"Data property {name} not found.");

            return dataProperty;
        }

        public virtual object? GetDataValue(string name)
        {
            var dataPropertyInfo = GetDataProperty(name);
            var dataInstance = dataPropertyInfo.GetValue(this);

            if (dataInstance is null)
                return null;

            var valuePropertyInfo = dataInstance.GetType().GetProperty("Value");

            if (valuePropertyInfo is null)
                throw new InvalidOperationException($"Data property {name} does not have a Value property.");

            return valuePropertyInfo.GetValue(dataInstance);
        }

        public virtual void SetDataValue(string name, object? value)
        {
            var dataPropertyInfo = GetDataProperty(name);
            var valuePropertyInfo = dataPropertyInfo.PropertyType.GetProperty("Value");

            if (valuePropertyInfo is null)
                throw new InvalidOperationException($"Data property {name} does not have a Value property.");

            var dataInstance = DataFactory.CreateData(valuePropertyInfo.PropertyType);

            valuePropertyInfo.SetValue(dataInstance, value);
            dataPropertyInfo.SetValue(this, dataInstance);
        }

        #endregion

        #region IConnectable

        public virtual IEnumerable<PropertyInfo> GetPortProperties()
        {
            // Get all properties of type Port
            var portProperties = GetType().GetProperties()
                .Where(p => p.PropertyType.IsAssignableTo(typeof(Port)))
                .AsEnumerable();

            if (portProperties is null || portProperties.Count() == 0)
                yield break;

            foreach (var portProperty in portProperties)
            {
                yield return portProperty;
            }
        }

        public virtual PropertyInfo GetPortProperty(string name)
        {
            var portPropertyInfo = GetType().GetProperty(name);

            if (portPropertyInfo is null)
                throw new InvalidOperationException($"Port property with name '{name}' doesn't exist.");

            if (!portPropertyInfo.PropertyType.IsAssignableTo(typeof(Port)))
                throw new InvalidOperationException($"Property with name '{name}' is not of type Port.");

            return portPropertyInfo;
        }

        public virtual void SetPortProperty(string name, Port value)
        {
            var property = GetPortProperty(name);

            property.SetValue(this, value);
        }

        public virtual IEnumerable<Port> GetPorts()
        {
            // Get all properties of type Port
            var portProperties = GetType().GetProperties()
                .Where(p => p.PropertyType.IsAssignableTo(typeof(Port)))
                .AsEnumerable();

            if (portProperties is null || portProperties.Count() == 0)
                yield break;

            foreach (var portProperty in portProperties)
            {
                var port = portProperty.GetValue(this) as Port;

                if (port is null)
                    continue;

                yield return port;
            }
        }

        public virtual void SetPortData(string name, object? value)
        {
            var property = GetPortProperty(name);

            if (property is null)
                throw new InvalidOperationException($"Port property with name '{name}' doesn't exist.");

            if (property.PropertyType.IsAssignableTo(typeof(Port)))
            {
                var port = property.GetValue(this) as Port;

                if (port is null)
                    throw new InvalidOperationException($"Port property with name '{name}' is null.");

                port.Data = value;
            }
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