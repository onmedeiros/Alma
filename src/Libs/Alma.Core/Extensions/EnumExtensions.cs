using Alma.Core.Types;

namespace Alma.Core.Domain.Shared.Extensions
{
    public static class EnumExtensions
    {
        public static IEnumerable<EnumItem> GetItems(this Enum @enum)
        {
            var enumType = @enum.GetType();
            var items = new List<EnumItem>();

            foreach (var value in EnumsNET.Enums.GetValues(enumType))
            {
                var name = EnumsNET.Enums.GetName(enumType, value) ??
                    throw new Exception("Impossible to determine name of enum value.");

                var description = EnumsNET.Enums.AsString(enumType, value, EnumsNET.EnumFormat.Description)
                    ?? name;

                items.Add(new EnumItem { Name = name, Value = description });
            }

            return items;
        }

        public static IEnumerable<EnumItem> GetItems(this Enum _, Type enumType)
        {
            var items = new List<EnumItem>();

            foreach (var value in EnumsNET.Enums.GetValues(enumType))
            {
                var name = EnumsNET.Enums.GetName(enumType, value) ??
                    throw new Exception("Impossible to determine name of enum value.");

                var description = EnumsNET.Enums.AsString(enumType, value, EnumsNET.EnumFormat.Description)
                    ?? name;

                items.Add(new EnumItem { Name = name, Value = description });
            }

            return items;
        }
    }
}