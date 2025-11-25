using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Utils;
using DotLiquid;
using Alma.Core.Extensions;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Alma.Workflows.Core.Activities.Base
{
    public class Parameter<T>
    {
        private static readonly Regex _templateRegex = new Regex(@"\$(var|param|now)\(([\w\.]*)\)", RegexOptions.Compiled);

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
                    return match.Groups[1].Value switch
                    {
                        "param" => ReplaceParameterTemplate(match.Groups[2].Value),
                        "var" => ReplaceVariableTemplate(match.Groups[2].Value),
                        "now" => ReplaceNowTemplate(),
                        _ => match.Value
                    };
                });

                var template = Template.Parse(processedValueStringTemplate);
                var templateVariables = context.State.AsTemplateData();
                var hash = Hash.FromDictionary(templateVariables);

                valueString = template.Render(hash, CultureInfo.InvariantCulture);
            }

            return ParameterValueConverter.Convert<T>(valueString);
        }

        public string ReplaceParameterTemplate(string value)
        {
            return $"{{{{ _parameter.{value} }}}}";
        }

        public string ReplaceVariableTemplate(string value)
        {
            return $"{{{{ _variable.{value} }}}}";
        }

        public string ReplaceNowTemplate()
        {
            return $"{DateTime.Now}";
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