using System.Globalization;

#pragma warning disable CS8603 // Possível retorno de referência nula.
#pragma warning disable CS8604 // Possível retorno de referência nula.
#pragma warning disable CS8602 // Possível retorno de referência nula.
namespace SimpleCore.Types
{
    public static  class TypeConvertExtensions
    {
        public static T ChangeType<T>(this object value, CultureInfo cultureInfo)
    {
        var toType = typeof(T);

            if (value == null) return default(T);

            if (value is string)
        {
            if (toType == typeof(Guid))
            {


                    return ChangeType<T>(new Guid(Convert.ToString(value, cultureInfo)), cultureInfo);
            }
            if ((string)value == string.Empty && toType != typeof(string))
            {
#pragma warning disable CS8625 // Não é possível converter um literal nulo em um tipo de referência não anulável.
                    return ChangeType<T>(null, cultureInfo);
#pragma warning restore CS8625 // Não é possível converter um literal nulo em um tipo de referência não anulável.
                }
        }
        else
        {
            if (typeof(T) == typeof(string))
            {
                return ChangeType<T>(Convert.ToString(value, cultureInfo), cultureInfo);
            }
        }

        if (toType.IsGenericType &&
            toType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            toType = Nullable.GetUnderlyingType(toType); ;
        }

        bool canConvert = toType is IConvertible || (toType.IsValueType && !toType.IsEnum);
        if (canConvert)
        {
            return (T)Convert.ChangeType(value, toType, cultureInfo);
        }
        return (T)value;
    }

    public static T ChangeType<T>(this object value)
    {
        return ChangeType<T>(value, CultureInfo.CurrentCulture);
    }
    }
}
#pragma warning restore CS8603 // Possível retorno de referência nula.
#pragma warning restore CS8604 // Possível retorno de referência nula.
#pragma warning restore CS8602 // Possível retorno de referência nula.