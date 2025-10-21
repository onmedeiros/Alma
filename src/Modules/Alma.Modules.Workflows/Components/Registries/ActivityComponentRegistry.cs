using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alma.Modules.Workflows.Components.Registries
{
    public interface IActivityComponentRegistry
    {
        Type? GetComponent(Type activityType);
        Type? GetComponent(string activityTypeFullName);
        bool TryGetComponent(Type activityType, out Type? componentType);
        bool TryGetComponent(string activityTypeFullName, out Type? componentType);
    }

    public class ActivityComponentRegistry : IActivityComponentRegistry
    {
        private readonly ILogger<ActivityComponentRegistry> _logger;
        private readonly ActivityComponentRegistryOptions _options;

        public ActivityComponentRegistry(ILogger<ActivityComponentRegistry> logger, IOptions<ActivityComponentRegistryOptions> options)
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
            return _options.ActivityComponents.TryGetValue(activityTypeFullName, out componentType);
        }
    }
}
