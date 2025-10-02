using Alma.Core.Types;
using EnumsNET;

namespace Alma.Core.Utils
{
    public static class EnumUtils
    {
        public static IEnumerable<EnumItem> GetItems(Enum @enum)
        {
            var enumType = @enum.GetType();
            return GetItems(enumType);
        }

        public static IEnumerable<EnumItem> GetItems(Type enumType)
        {
            var items = new List<EnumItem>();

            foreach (var value in Enums.GetValues(enumType))
            {
                var valueName = Enums.GetName(enumType, value) ??
                    throw new Exception("Impossible to determine name of enum value.");

                var name = Enums.AsString(enumType, value, EnumFormat.Description)
                    ?? valueName;

                items.Add(new EnumItem { Name = name, Value = valueName });
            }

            return items;
        }

        public static IEnumerable<EnumItem<TEnum>> GetItems<TEnum>() where TEnum : struct, Enum
        {
            var enumType = typeof(TEnum);

            return GetItems(enumType).Select(item => new EnumItem<TEnum>
            {
                Name = item.Name,
                Value = (TEnum)Enum.Parse(enumType, item.Value)
            });
        }
    }
}