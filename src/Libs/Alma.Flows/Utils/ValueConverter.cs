using Alma.Flows.Core.Activities.Models;
using Alma.Flows.Models.Activities;
using System.Text.Json;

namespace Alma.Flows.Utils
{
    public static class ValueConverter
    {
        public static T? Convert<T>(string? value)
        {
            return (T?)Convert(typeof(T), value);
        }

        public static object? Convert(Type valueType, string? value)
        {
            try
            {
                object? convertedValue = null;

                if (value is null)
                {
                    convertedValue = null;
                }
                else if (valueType == typeof(string))
                {
                    convertedValue = System.Convert.ToString(value);
                }
                else if (valueType == typeof(int))
                {
                    convertedValue = System.Convert.ToInt32(value);
                }
                else if (valueType == typeof(long))
                {
                    convertedValue = System.Convert.ToInt64(value);
                }
                else if (valueType == typeof(double))
                {
                    convertedValue = System.Convert.ToDouble(value);
                }
                else if (valueType == typeof(decimal))
                {
                    convertedValue = System.Convert.ToDecimal(value);
                }
                else if (valueType == typeof(bool))
                {
                    convertedValue = System.Convert.ToBoolean(value);
                }
                else if (valueType == typeof(DateTime))
                {
                    convertedValue = System.Convert.ToDateTime(value);
                }
                else if (valueType.IsAssignableTo(typeof(Enum)))
                {
                    if (string.IsNullOrEmpty(value))
                        convertedValue = Enum.GetValues(valueType).GetValue(0);
                    else
                        convertedValue = Enum.Parse(valueType, value, true);
                }
                else if (valueType == typeof(Dictionary<string, string>))
                {
                    convertedValue = JsonSerializer.Deserialize<Dictionary<string, string>>(string.IsNullOrEmpty(value) ? "{}" : value);
                }
                else if (valueType == typeof(ICollection<FormField>))
                {
                    convertedValue = JsonSerializer.Deserialize<ICollection<FormField>>(string.IsNullOrEmpty(value) ? "[]" : value);
                }
                else if (valueType == typeof(ParameterOption))
                {
                    convertedValue = new ParameterOption { Value = value, DisplayName = string.Empty };
                }
                else
                {
                    throw new InvalidOperationException($"Failed to convert value. Value type '{valueType.FullName}' not supported.");
                }

                return convertedValue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert parameter value type {valueType}.", ex);
            }
        }
    }
}