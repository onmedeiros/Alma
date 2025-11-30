using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Factories;
using Alma.Workflows.Core.Description.Descriptors;
using Alma.Workflows.Registries;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Builders
{
    public interface IActivityBuilder
    {
        ActivityBuilder Begin(string fullName, string? id = null, string? discriminator = null);

        ActivityBuilder Begin(ActivityDescriptor descriptor, string? id = null, string? discriminator = null);

        IActivityBuilder WithDisplayName(string displayName);

        ActivityBuilder WithParameter(Type type, string name, object? value);

        IActivityBuilder WithApprovalAndCheck(Type type, Action<ApprovalAndCheckBuilder> builder);

        IActivityBuilder WithBeforeExecutionStep<T>(string? id = null) where T : IStep;

        IActivityBuilder WithBeforeExecutionStep(Type type, string? id = null);

        IActivityBuilder WithAfterExecutionStep<T>(string? id = null) where T : IStep;

        IActivityBuilder WithAfterExecutionStep(Type type, string? id = null);

        IActivity Build();

        IActivity Activity { get; }
    }

    public class ActivityBuilder : IActivityBuilder
    {
        private IActivity? _activity;

        protected readonly ILogger<ActivityBuilder> Logger;
        protected readonly IActivityRegistry ActivityRegistry;
        protected readonly IApprovalAndCheckRegistry ApprovalAndCheckRegistry;
        protected readonly ICustomActivityRegistry CustomActivityRegistry;
        protected readonly IActivityStepFactory ActivityStepFactory;

        public IActivity Activity
        {
            get
            {
                return _activity ?? throw new Exception("Activity not initialized.");
            }
            private set
            {
                _activity = value;
            }
        }

        public ActivityBuilder(ILogger<ActivityBuilder> logger, IActivityRegistry activityRegistry, IApprovalAndCheckRegistry approvalAndCheckRegistry, ICustomActivityRegistry customActivityRegistry, IActivityStepFactory activityStepFactory)
        {
            Logger = logger;
            ActivityRegistry = activityRegistry;
            ApprovalAndCheckRegistry = approvalAndCheckRegistry;
            CustomActivityRegistry = customActivityRegistry;
            ActivityStepFactory = activityStepFactory;
        }

        public virtual ActivityBuilder Begin(string fullActivityName, string? id = null, string? discriminator = null)
        {
            ActivityDescriptor? descriptor = null;

            // Search for the activity descriptor.
            if (fullActivityName.StartsWith("Alma.Workflows.Core.CustomActivities.CustomActivity"))
            {
                descriptor = CustomActivityRegistry.GetActivityDescriptorAsync(fullActivityName, discriminator).GetAwaiter().GetResult();
            }
            else
            {
                descriptor = ActivityRegistry.GetActivityDescriptor(fullActivityName);
            }

            if (descriptor is null)
                throw new InvalidOperationException($"Activity with Full Activity Name {descriptor} not registered.");

            return Begin(descriptor, id, discriminator);
        }

        public virtual ActivityBuilder Begin(ActivityDescriptor descriptor, string? id = null, string? discriminator = null)
        {
            // Create Activity instance.
            if (Activator.CreateInstance(descriptor.Type) is not IActivity instance)
                throw new InvalidOperationException($"Failed to create an instance of {descriptor.Type}.");

            if (!string.IsNullOrEmpty(id))
                instance.Id = id;

            instance.SetDescriptor(descriptor);

            // Create Activity Ports.
            foreach (var portDescriptor in descriptor.Ports)
            {
                var port = new Port
                {
                    Activity = instance,
                    Descriptor = portDescriptor,
                    Type = portDescriptor.Type,
                    DataType = portDescriptor.DataType
                };

                instance.SetPortProperty(portDescriptor.Name, port);
            }

            Activity = instance;

            return this;
        }

        public ActivityBuilder WithDisplayName(string displayName)
        {
            Activity.DisplayName = displayName;
            return this;
        }

        public ActivityBuilder WithParameter(Type type, string name, object? value)
        {
            Activity.SetParameterValue(name, value);
            return this;
        }

        public IActivityBuilder WithApprovalAndCheck(Type type, Action<ApprovalAndCheckBuilder> builder)
        {
            var approvalAndCheckDescriptor = ApprovalAndCheckRegistry.GetApprovalAndCheckDescriptor(type);
            var approvalAndCheckBuilder = new ApprovalAndCheckBuilder(approvalAndCheckDescriptor);

            builder(approvalAndCheckBuilder);

            Activity!.ApprovalAndChecks.Add(approvalAndCheckBuilder.Instance);

            return this;
        }

        public IActivityBuilder WithBeforeExecutionStep<T>(string? id = null) where T : IStep
        {
            return WithBeforeExecutionStep(typeof(T), id);
        }

        public IActivityBuilder WithBeforeExecutionStep(Type type, string? id = null)
        {
            var stepCount = Activity.BeforeExecutionSteps.Count;
            var step = ActivityStepFactory.Create(type);

            if (string.IsNullOrEmpty(id))
                id = $"{type.Name}_{stepCount}";

            step.SetId(id);
            step.SetActivity(Activity);

            Activity.BeforeExecutionSteps.Add(step);

            return this;
        }

        public IActivityBuilder WithAfterExecutionStep<T>(string? id = null) where T : IStep
        {
            return WithAfterExecutionStep(typeof(T));
        }

        public IActivityBuilder WithAfterExecutionStep(Type type, string? id = null)
        {
            var stepCount = Activity.AfterExecutionSteps.Count;
            var step = ActivityStepFactory.Create(type);

            if (string.IsNullOrEmpty(id))
                id = $"{type.Name}_{stepCount}";

            step.SetId(id);
            step.SetActivity(Activity);

            Activity.AfterExecutionSteps.Add(step);

            return this;
        }

        public virtual IActivity Build()
        {
            return Activity
                ?? throw new Exception("Activity couldn't be constructed. Has the method Begin have been called?");
        }

        IActivityBuilder IActivityBuilder.WithDisplayName(string displayName)
        {
            return WithDisplayName(displayName);
        }
    }
}