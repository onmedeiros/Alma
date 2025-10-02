namespace Alma.Core.Extensions
{
    public static class DoubleExtensions
    {
        public static double Round(this double value, int decimalPlaces)
        {
            var rounded = Math.Round(value, 2);

            return rounded;
        }

        public static double? Round(this double? value, int decimalPlaces)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return Math.Round(value.Value, 2);
        }
    }
}
