using Alma.Workflows.Core.CustomActivities.Services;
using Alma.Workflows.Core.CustomActivities.Stores;
using Alma.Workflows.Core.Description.Describers;
using Alma.Workflows.Core.Description.Descriptors;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Registries
{
    public interface ICustomActivityRegistry
    {
        ValueTask<IReadOnlyCollection<ActivityDescriptor>> ListActivityDescriptorsAsync(string? discriminator = null);

        ValueTask<ActivityDescriptor?> GetActivityDescriptorAsync(string fullName, string? discriminator = null);
    }

    public class CustomActivityRegistry : ICustomActivityRegistry
    {
        private readonly ILogger<CustomActivityRegistry> _logger;
        private readonly ICustomActivityManager _customActivityManager;

        private readonly ICollection<ActivityDescriptor> _activityDescriptors = [];

        private bool _listLoaded = false;

        public CustomActivityRegistry(ILogger<CustomActivityRegistry> logger, ICustomActivityManager customActivityManager)
        {
            _logger = logger;
            _customActivityManager = customActivityManager;
        }

        public async ValueTask<IReadOnlyCollection<ActivityDescriptor>> ListActivityDescriptorsAsync(string? discriminator = null)
        {
            if (_listLoaded)
                return (IReadOnlyCollection<ActivityDescriptor>)_activityDescriptors;

            _logger.LogDebug("Loading custom activities with discriminator {Discriminator}.", discriminator);

            var activities = await _customActivityManager.List(1, int.MaxValue, new CustomActivityFilters
            {
                Discriminator = discriminator
            });

            foreach (var activity in activities)
                _activityDescriptors.Add(ActivityDescriber.Describe(activity));

            _listLoaded = true;

            return _activityDescriptors.ToList().AsReadOnly();
        }

        public async ValueTask<ActivityDescriptor?> GetActivityDescriptorAsync(string fullName, string? discriminator = null)
        {
            if (!_listLoaded)
                await ListActivityDescriptorsAsync(discriminator);

            var activity = _activityDescriptors.FirstOrDefault(a => a.FullName == fullName);

            if (activity is null)
            {
                _logger.LogError("Activity with fullName {FullName} not found.", fullName);
                throw new Exception($"Activity with fullName {fullName} not found.");
            }

            return activity;
        }
    }
}