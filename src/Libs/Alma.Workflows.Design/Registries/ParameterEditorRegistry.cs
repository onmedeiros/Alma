using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alma.Workflows.Design.Registries
{
    public interface IParameterEditorRegistry
    {
        Type? GetComponent(Type activityType);

        Type? GetComponent(string activityTypeFullName);

        bool TryGetComponent(Type activityType, out Type? componentType);

        bool TryGetComponent(string activityTypeFullName, out Type? componentType);
    }

    public class ParameterEditorRegistry : IParameterEditorRegistry
    {
        private readonly ILogger<ParameterEditorRegistry> _logger;
        private readonly ParameterEditorRegistryOptions _options;

        public ParameterEditorRegistry(ILogger<ParameterEditorRegistry> logger, IOptions<ParameterEditorRegistryOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public Type? GetComponent(Type activityType)
        {
            return GetComponent(activityType.FullName ?? string.Empty);
        }

        public Type? GetComponent(string activityTypeFullName)
        {
            TryGetComponent(activityTypeFullName, out var componentType);
            return componentType;
        }

        public bool TryGetComponent(Type activityType, out Type? componentType)
        {
            return TryGetComponent(activityType.FullName ?? string.Empty, out componentType);
        }

        public bool TryGetComponent(string activityTypeFullName, out Type? componentType)
        {
            return _options.Components.TryGetValue(activityTypeFullName, out componentType);
        }
    }
}