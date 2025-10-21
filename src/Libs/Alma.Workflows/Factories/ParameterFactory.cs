using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Models.Activities;
using Alma.Core.Extensions;
using System.Text.Json;

namespace Alma.Workflows.Factories
{
    public static class ParameterFactory
    {
        public static object CreateParameter(Type type)
        {
            var parameterInstance = Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Failed to create an instance of {type}.");

            return parameterInstance;
        }

        public static object? CreateParameterValue(Type valueType, object? value)
        {
            try
            {
                object? convertedValue = valueType switch
                {
                    Type t when t == typeof(int) => Convert.ToInt32(string.IsNullOrEmpty(value?.ToString()) ? null : value),
                    Type t when t == typeof(double) => Convert.ToDouble(value),
                    Type t when t == typeof(bool) => Convert.ToBoolean(string.IsNullOrEmpty(value?.ToString()) ? null : value),
                    Type t when t == typeof(decimal) => Convert.ToDecimal(string.IsNullOrEmpty(value?.ToString()) ? null : value),
                    Type t when t == typeof(string) => Convert.ToString(value),
                    Type t when t == typeof(DateTime) => Convert.ToDateTime(value),
                    Type t when t == typeof(Dictionary<string, string>) => JsonSerializer.Deserialize<Dictionary<string, string>>(value?.ToString().IsNullOrEmpty("{}")),
                    Type t when t == typeof(ICollection<FormField>) => JsonSerializer.Deserialize<ICollection<FormField>>(string.IsNullOrEmpty(value?.ToString()) ? "[]" : value?.ToString()),
                    Type t when t.IsAssignableTo(typeof(Enum)) => string.IsNullOrEmpty(value?.ToString()) ? Enum.GetValues(t).GetValue(0) : Enum.Parse(t, value.ToString()!),
                    _ => value
                };

                return convertedValue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert parameter value type {valueType}.", ex);
            }
        }

        public static object CreateParameter(Type type, object? value)
        {
            // Cria uma instância do parâmetro
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var parameterType = typeof(Parameter<>).MakeGenericType(type);
            var parameterInstance = Activator.CreateInstance(parameterType);

            if (parameterInstance == null)
                throw new InvalidOperationException($"Failed to create an instance of {parameterType}.");

            // Atribui o valor do parâmetro em string
            var valueStringProperty = parameterType.GetProperty(nameof(Parameter<object>.ValueString))
                ?? throw new InvalidOperationException($"Property 'ValueString' not found on {parameterType}.");

            valueStringProperty.SetValue(parameterInstance, value?.ToString());

            return parameterInstance;

            // Se o valor passado for string, verifica se é um template.
            // Se não, converte o valor e atribui ao parâmetro.
            //if (value is string stringValue)
            //{
            //    var valueTemplateProperty = parameterType.GetProperty(nameof(Parameter<object>.ValueTemplate));

            //    if (valueTemplateProperty == null)
            //        throw new InvalidOperationException($"Property 'ValueTemplate' not found on {parameterType}.");

            //    valueTemplateProperty.SetValue(parameterInstance, stringValue);
            //}
            //else
            //{
            //    var valueProperty = parameterType.GetProperty(nameof(Parameter<object>.Value));

            //    if (valueProperty == null)
            //        throw new InvalidOperationException($"Property 'Value' not found on {parameterType}.");

            //    var valuePropertyType = valueProperty.PropertyType;

            //    try
            //    {
            //        object? convertedValue = valuePropertyType switch
            //        {
            //            Type t when t == typeof(int) => Convert.ToInt32(string.IsNullOrEmpty(value?.ToString()) ? null : value),
            //            Type t when t == typeof(double) => Convert.ToDouble(value),
            //            Type t when t == typeof(bool) => Convert.ToBoolean(string.IsNullOrEmpty(value?.ToString()) ? null : value),
            //            Type t when t == typeof(decimal) => Convert.ToDecimal(string.IsNullOrEmpty(value?.ToString()) ? null : value),
            //            Type t when t == typeof(string) => Convert.ToString(value),
            //            Type t when t == typeof(DateTime) => Convert.ToDateTime(value),
            //            Type t when t == typeof(Dictionary<string, string>) => JsonSerializer.Deserialize<Dictionary<string, string>>(value?.ToString() ?? "{}"),
            //            Type t when t.IsAssignableTo(typeof(Enum)) => string.IsNullOrEmpty(value?.ToString()) ? Enum.GetValues(t).GetValue(0) : Enum.Parse(t, value.ToString()!),
            //            _ => value
            //        };

            //        valueProperty.SetValue(parameterInstance, convertedValue);
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new InvalidOperationException($"Failed to convert and set value on parameter type {parameterType}.", ex);
            //    }
            //}

            //return parameterInstance;
        }
    }
}