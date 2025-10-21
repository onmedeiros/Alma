using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Core.CustomActivities.Services;
using Alma.Workflows.Scripting;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Alma.Workflows.Core.CustomActivities
{
    public class CustomActivity : Activity
    {
        public ICollection<CustomActivityParameter> Parameters { get; set; } = [];

        public ICollection<Port> Ports { get; set; } = [];

        #region IParametrizable

        public override PropertyInfo GetParameterProperty(string name)
        {
            throw new Exception("CustomActivity does not support this method.");
        }

        public override TValue GetParameterValue<TValue>(string name, ActivityExecutionContext context)
        {
            var parameter = Parameters.FirstOrDefault(p => p.Name == name);

            if (parameter is null)
                throw new Exception($"Parameter '{name}' not found.");

            object? value = parameter.Value;
            if (value is null)
                return default!;

            var targetType = typeof(TValue);
            var valueType = value.GetType();

            // If already correct type
            if (value is TValue tValue)
                return tValue;

            // Handle Nullable<T>
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                if (underlyingType.IsEnum)
                {
                    if (value is string s)
                        return (TValue)Enum.Parse(underlyingType, s);
                    return (TValue)Enum.ToObject(underlyingType, value);
                }
                if (underlyingType == typeof(Guid))
                {
                    if (value is Guid g)
                        return (TValue)(object)g;
                    if (value is string s)
                        return (TValue)(object)Guid.Parse(s);
                }
                // Use Convert.ChangeType for primitives
                return (TValue)Convert.ChangeType(value, underlyingType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Parameter '{name}' could not be converted to type {targetType}.", ex);
            }
        }

        public override string? GetParameterValueAsString(string name)
        {
            var parameter = Parameters.FirstOrDefault(p => p.Name == name);

            if (parameter is null)
                throw new Exception($"Parameter '{name}' not found.");

            if (parameter.Value is null)
                return null;

            return parameter.Value.ToString();
        }

        public override void SetParameterValue(string name, object? value)
        {
            var parameter = Parameters.FirstOrDefault(p => p.Name == name);

            if (parameter is null)
            {
                parameter = new CustomActivityParameter(value)
                {
                    Name = name
                };

                Parameters.Add(parameter);
            }
            else
            {
                parameter.Value = value;
            }
        }

        #endregion

        #region IConnectable

        public override void SetPortProperty(string name, Port port)
        {
            var existingPort = Ports.FirstOrDefault(p => p.Descriptor.Name == name);

            if (existingPort is not null)
                Ports.Remove(existingPort);

            Ports.Add(port);
        }

        public override IEnumerable<Port> GetPorts()
        {
            return Ports;
        }

        #endregion

        public override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            // Custom activity data
            var customActivityId = Descriptor.FullName.Split("+")[1];

            // Run script
            var customActivityManager = context.ServiceProvider.GetRequiredService<ICustomActivityManager>();

            var script = await customActivityManager.FindScriptAsync(activityId: customActivityId, discriminator: null);

            var engine = context.ServiceProvider.GetRequiredKeyedService<IScriptEngine>(ScriptLanguage.JavaScript);

            foreach (var parameter in Parameters)
            {
                engine.SetParameter(parameter.Name, parameter.Value);
            }

            var result = await engine.ExecuteAsync(script.Content, cancellationToken: CancellationToken.None);

            if (result.Logs.Count > 0)
            {
                foreach (var log in result.Logs)
                {
                    context.State.Logs.Add(new Workflows.Models.Activities.LogModel
                    {
                        DateTime = log.Timestamp,
                        Message = log.Message,
                        Severity = Enums.LogSeverity.Information
                    });
                }
            }

            if (result.Status == ScriptResultStatus.Failure)
            {
                context.State.Log(result.StatusDetails, Enums.LogSeverity.Error);
                return;
            }

            // Check executed ports
            if (result.ExecutedPorts.Count > 0)
            {
                foreach (var executedPort in result.ExecutedPorts)
                {
                    var port = Ports.FirstOrDefault(p => p.Descriptor.Name == executedPort.Name);

                    if (port is null)
                        throw new Exception($"Port '{executedPort.Name}' not found.");

                    // Execute the port with the provided data
                    port.Execute(executedPort.Data);
                }
            }
        }
    }
}