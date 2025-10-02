using Alma.Flows.Core.Common.Enums;

namespace Alma.Flows.Core.Common.Extensions
{
    public static class ParameterTypeExtensions
    {
        public static string GetParameterTypeName(this ParameterType type)
        {
            return type switch
            {
                ParameterType.String => typeof(string).Name,
                ParameterType.Int => typeof(int).Name,
                ParameterType.Bool => typeof(bool).Name,
                ParameterType.Decimal => typeof(decimal).Name,
                ParameterType.DateTime => typeof(DateTime).Name,
                _ => typeof(string).Name
            };
        }

        public static Type GetParameterType(this ParameterType type)
        {
            return type switch
            {
                ParameterType.String => typeof(string),
                ParameterType.Int => typeof(int),
                ParameterType.Bool => typeof(bool),
                ParameterType.Decimal => typeof(decimal),
                ParameterType.DateTime => typeof(DateTime),
                _ => typeof(string)
            };
        }
    }
}