using Alma.Workflows.Core.States.Abstractions;
using Alma.Workflows.Utils;
using DotLiquid;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Alma.Workflows.Core.Templates
{
    public interface ITemplateParser
    {
        string? Parse(string content);

        T? Parse<T>(string content);
    }

    public class TemplateParser : ITemplateParser
    {
        private static readonly Regex _templateRegex = new(@"\$(var|param|now)\(([\w\.]*)\)", RegexOptions.Compiled);

        private readonly ILogger<TemplateParser> _logger;
        private readonly IExecutionState _state;

        public TemplateParser(ILogger<TemplateParser> logger, IExecutionState state)
        {
            _logger = logger;
            _state = state;
        }

        public string? Parse(string content)
        {
            if (content is null)
                return default;

            if (!IsTemplate(content))
                return content;

            var processedContent = _templateRegex.Replace(content, match =>
            {
                return match.Groups[1].Value switch
                {
                    "param" => ReplaceParameterTemplate(match.Groups[2].Value),
                    "var" => ReplaceVariableTemplate(match.Groups[2].Value),
                    "now" => ReplaceNowTemplate(),
                    _ => match.Value
                };
            });

            var template = Template.Parse(processedContent);
            var templateData = _state.AsTemplateData();
            var templateDataHash = Hash.FromDictionary(templateData);

            return template.Render(templateDataHash);
        }

        public T? Parse<T>(string content)
        {
            var parsedContent = Parse(content);

            if (parsedContent is null)
                return default;

            return ParameterValueConverter.Convert<T>(parsedContent);
        }

        public static bool IsTemplate(string content)
        {
            if (content is null) return false;

            return _templateRegex
                .IsMatch(content);
        }

        private static string ReplaceParameterTemplate(string value)
        {
            return $"{{{{ _parameter.{value} }}}}";
        }

        private static string ReplaceVariableTemplate(string value)
        {
            return $"{{{{ _variable.{value} }}}}";
        }

        private static string ReplaceNowTemplate()
        {
            return $"{DateTime.Now}";
        }
    }
}