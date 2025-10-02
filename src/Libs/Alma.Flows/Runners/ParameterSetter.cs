using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Alma.Flows.Runners
{
    /// <summary>
    /// Interface for setting parameters in a parameterizable object.
    /// </summary>
    public interface IParameterSetter
    {
        /// <summary>
        /// Sets the parameters for the given parameterizable object based on the provided context.
        /// </summary>
        /// <param name="context">The execution context containing state and options.</param>
        /// <param name="parameterizable">The parameterizable object whose parameters need to be set.</param>
        // void SetParameters(ActivityExecutionContext context, IParameterizable parameterizable);
    }

    /// <summary>
    /// Implementation of <see cref="IParameterSetter"/> that sets parameters using templates.
    /// </summary>
    public class ParameterSetter : IParameterSetter
    {
        private readonly ILogger<ParameterSetter> _logger;

        private static Regex _templateRegex = new Regex(@"\$(var|param)\(([\w\.]+)\)", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSetter"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public ParameterSetter(ILogger<ParameterSetter> logger)
        {
            _logger = logger;
        }

        ///// <inheritdoc />
        //public void SetParameters(ActivityExecutionContext context, IParameterizable parameterizable)
        //{
        //    foreach (var parameterDescriptor in parameterizable.ParameterDescriptors)
        //    {
        //        _logger.LogDebug("Setting parameter '{ParameterName}'.", parameterDescriptor.Name);

        //        var valueTemplate = parameterizable.GetParameterTemplate(parameterDescriptor.Name);

        //        if (string.IsNullOrEmpty(valueTemplate))
        //            continue;

        //        var value = string.Empty;

        //        if (valueTemplate.IsTemplate())
        //        {
        //            // Prepare the template for rendering
        //            string processedValueTemplate = _templateRegex.Replace(valueTemplate, match =>
        //            {
        //                string prefix = match.Groups[1].Value == "var" ? "_variable" : "_parameter";

        //                return $"{{{{ {prefix}.{match.Groups[2].Value} }}}}";
        //            });

        //            var template = Template.Parse(processedValueTemplate);

        //            value = template.Render(Hash.FromDictionary(context.State.GetTemplateVariables()), CultureInfo.InvariantCulture);
        //        }
        //        else
        //        {
        //            value = valueTemplate;
        //        }

        //        parameterizable.SetParameterValue(parameterDescriptor.Name, value);
        //    }
        //}
    }
}