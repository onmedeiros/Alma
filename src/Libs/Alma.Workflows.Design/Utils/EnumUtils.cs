namespace Alma.Workflows.Design.Utils
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

            foreach (var value in EnumsNET.Enums.GetValues(enumType))
            {
                var valueName = EnumsNET.Enums.GetName(enumType, value) ??
                    throw new Exception("Impossible to determine name of enum value.");

                var name = EnumsNET.Enums.AsString(enumType, value, EnumsNET.EnumFormat.Description)
                    ?? valueName;

                items.Add(new EnumItem { Name = name, Value = valueName });
            }

            return items;
        }
    }

    public class EnumItem
    {
        public required string Name { get; set; }
        public required string Value { get; set; }
    }
}
