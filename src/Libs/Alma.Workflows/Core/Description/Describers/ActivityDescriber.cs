using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.CustomActivities;
using Alma.Workflows.Core.CustomActivities.Entities;
using Alma.Workflows.Core.Description.Descriptors;
using Alma.Core.Extensions;
using Alma.Workflows.Core.Activities.Abstractions;

namespace Alma.Workflows.Core.Description.Describers
{
    public static class ActivityDescriber
    {
        public static ActivityDescriptor Describe(Type activityType)
        {
            if (activityType is null)
                throw new ArgumentNullException(nameof(activityType));

            if (!activityType.IsAssignableTo(typeof(IActivity)))
                throw new Exception("Type must be of type Activity");

            var attributes = activityType.GetCustomAttributes(true);
            var activityAttribute = attributes.OfType<ActivityAttribute>().FirstOrDefault();

            var parameters = activityType.GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Parameter<>))
                .AsEnumerable();

            var ports = activityType.GetProperties()
                .Where(p => p.PropertyType.IsAssignableTo(typeof(Port)))
                .AsEnumerable();

            var name = activityType.Name;
            var typeName = activityType.FullName ?? throw new Exception("Invalid activity FullName");

            var @namespace = string.Empty;
            var category = string.Empty;
            var displayName = string.Empty;
            var description = string.Empty;
            var requireInteraction = false;

            if (activityAttribute != null)
            {
                @namespace = activityAttribute.Namespace.IsNullOrEmpty(activityType.Namespace!)
                    ?? throw new Exception("Invalid activity namespace.");

                category = activityAttribute.Category.IsNullOrEmpty(activityType.Namespace!)
                    ?? throw new Exception("Invalid activity namespace.");

                displayName = activityAttribute.DisplayName.IsNullOrEmpty(name);

                description = activityAttribute.Description;

                requireInteraction = activityAttribute.RequireInteraction;
            }
            else
            {
                @namespace = activityType.Namespace ?? throw new Exception("Invalid activity namespace.");
                category = activityType.Namespace;
                displayName = name;
            }

            var descriptor = new ActivityDescriptor
            {
                Type = activityType,
                TypeName = typeName,
                Namespace = @namespace,
                Category = category,
                Name = name,
                FullName = $"{@namespace}.{name}",
                DisplayName = displayName,
                Description = description,
                RequireInteraction = requireInteraction,
                Attributes = attributes,
                Parameters = parameters.Select(ParameterDescriber.Describe).ToList(),
                Ports = ports.Select(PortDescriber.Describe).ToList()
            };

            return descriptor;
        }

        public static ActivityDescriptor Describe(CustomActivityTemplate activity)
        {
            var type = typeof(CustomActivity);

            var descriptor = new ActivityDescriptor
            {
                Type = type,
                TypeName = $"{type.FullName}+{activity.Id}",
                Namespace = type.Namespace ?? throw new Exception("Invalid activity namespace."),
                Category = activity.CategoryId ?? "other",
                Name = $"{type.Name}+{activity.Id}",
                FullName = $"{type.FullName}+{activity.Id}",
                DisplayName = activity.Name,
                Description = activity.Description,
                Parameters = activity.Parameters.Select(ParameterDescriber.Describe).ToList(),
                Ports = activity.Ports.Select(PortDescriber.Describe).ToList(),
            };

            return descriptor;
        }
    }
}