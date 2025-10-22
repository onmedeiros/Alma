using System.Text.RegularExpressions;

namespace Alma.Core.Extensions
{
    public static class StringExtensions
    {
        public static string IfNull(this string? value, string replace)
        {
            if (string.IsNullOrEmpty(value))
                return replace;

            return value;
        }

        public static string Normalized(this string value)
        {
            return value.ToUpperInvariant().Trim();
        }

        public static string IsNullOrEmpty(this string? value, string replacer)
        {
            return string.IsNullOrEmpty(value) ? replacer : value;
        }

        public static bool IsTemplate(this string value)
        {
            return value.Contains("$var(") || value.Contains("$param(") || value.Contains("$now()");
        }

        public static bool ContainsLogicalOperator(this string value)
        {
            // Expressão regular que identifica operadores lógicos comuns.
            // Ela procura por:
            // - Operadores simbólicos: &&, ||, ==, !=, >=, <=, >, <
            // - Palavras reservadas: and, or, not, xor (case insensitive)
            string logicalOperatorsPattern = @"(&&|\|\||==|!=|>=|<=|>|<|\b(and|or|not|xor)\b)";

            // Se encontrar algum operador lógico, lança um erro ou interrompe a avaliação.
            return Regex.IsMatch(value, logicalOperatorsPattern, RegexOptions.IgnoreCase);
        }

        public static string ReplaceTemplate(this string str, string key, string value)
        {
            string pattern = @"\{\s*\{\s*" + key + @"\s*\}\s*\}";

            return Regex.Replace(str, pattern, value, RegexOptions.IgnoreCase);
        }
    }
}