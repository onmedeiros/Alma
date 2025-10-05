using Alma.Flows.Core.Contexts;
using Alma.Flows.Utils;
using DotLiquid;
using Alma.Core.Extensions;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Alma.Flows.Core.Activities.Base
{
    public class Parameter<T>
    {
        private static Regex _templateRegex = new Regex(@"\$(var|param)\(([\w\.]+)\)", RegexOptions.Compiled);

        public string? ValueString { get; private set; }

        public Parameter()
        {
        }

        public Parameter(T? value)
        {
            if (value is not null)
                ValueString = value.ToString();
        }

        public T? GetValue(ActivityExecutionContext context)
        {
            if (ValueString is null)
                return default;

            var valueString = ValueString;

            // Process Value string if is template.
            if (ValueString.IsTemplate())
            {
                var processedValueStringTemplate = _templateRegex.Replace(ValueString, match =>
                {
                    string prefix = match.Groups[1].Value == "var" ? "_variable" : "_parameter";

                    return $"{{{{ {prefix}.{match.Groups[2].Value} }}}}";
                });

                var template = Template.Parse(processedValueStringTemplate);

                valueString = template.Render(Hash.FromDictionary(context.State.GetTemplateVariables()), CultureInfo.InvariantCulture);
            }

            return ValueConverter.Convert<T>(valueString);
        }

        public void SetValue(T? value)
        {
            if (value is not null)
                ValueString = value.ToString();
            else
                ValueString = null;
        }
    }
}