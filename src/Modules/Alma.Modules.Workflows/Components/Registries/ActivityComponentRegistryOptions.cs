using Alma.Workflows.Core.Activities.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Alma.Modules.Workflows.Components.Registries
{
    public class ActivityComponentRegistryOptions
    {
        private Dictionary<string, Type> _activityComponents = new Dictionary<string, Type>();

        public IReadOnlyDictionary<string, Type> ActivityComponents => _activityComponents;

        public ActivityComponentRegistryOptions()
        {

        }

        public void Register(string name, Type type)
        {
            _activityComponents.Add(name, type);
        }

        public void Register<TActivity, TComponent>()
            where TActivity : IActivity
            where TComponent : ComponentBase
        {
            var name = typeof(TActivity).FullName;

            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Activity type name is empty.");

            if (_activityComponents.ContainsKey(name))
                throw new InvalidOperationException($"Activity component for {name} already registered.");

            Register(name, typeof(TComponent));
        }
    }
}
