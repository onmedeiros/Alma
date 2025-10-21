using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Description.Describers;
using Alma.Workflows.Core.Description.Descriptors;
using Alma.Workflows.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alma.Workflows.Registries
{
    public interface IActivityRegistry
    {
        IEnumerable<ActivityDescriptor> ActivityDescriptors { get; }

        void RegisterActivity(Type type);

        ActivityDescriptor GetActivityDescriptor(string fullName);

        ActivityDescriptor GetActivityDescriptor(Type type);

        ActivityDescriptor GetActivityDescriptor<TActivity>()
            where TActivity : class, IActivity;
    }

    public class ActivityRegistry : IActivityRegistry
    {
        private readonly ILogger<ActivityRegistry> _logger;
        private readonly FlowOptions _options;

        private readonly ICollection<ActivityDescriptor> _activityDescriptors = [];

        public IEnumerable<ActivityDescriptor> ActivityDescriptors => _activityDescriptors;

        public ActivityRegistry(ILogger<ActivityRegistry> logger, IOptions<FlowOptions> options)
        {
            _logger = logger;
            _options = options.Value;

            foreach (var type in _options.ActivityTypes)
                RegisterActivity(type);
        }

        public void RegisterActivity(Type type)
        {
            _logger.LogDebug("Registering activity type {Type}.", type.FullName);

            if (!type.IsAssignableTo(typeof(IActivity)))
            {
                _logger.LogError($"Type {type.FullName} does not implement IActivity.");
                throw new ArgumentException($"Type {type.FullName} does not implement IActivity.");
            }

            _activityDescriptors.Add(ActivityDescriber.Describe(type));
        }

        public ActivityDescriptor GetActivityDescriptor(Type type)
        {
            return _activityDescriptors.FirstOrDefault(x => x.TypeName == type.FullName)
                ?? throw new Exception($"Descriptor for Type Full Name {type.FullName} not found.");
        }

        public ActivityDescriptor GetActivityDescriptor(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                _logger.LogError("FullName cannot be null or empty.");
                throw new ArgumentNullException(nameof(fullName), "FullName cannot be null or empty.");
            }

            return _activityDescriptors.FirstOrDefault(x => x.FullName == fullName)
                ?? throw new Exception($"Descriptor for {fullName} not found.");
        }

        public ActivityDescriptor GetActivityDescriptor<TActivity>()
            where TActivity : class, IActivity
        {
            return GetActivityDescriptor(typeof(TActivity));
        }
    }
}