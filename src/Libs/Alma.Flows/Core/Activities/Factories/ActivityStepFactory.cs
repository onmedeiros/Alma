using Alma.Flows.Core.Activities.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Flows.Core.Activities.Factories
{
    public interface IActivityStepFactory
    {
        IStep Create<TStep>() where TStep : class, IStep;

        IStep Create(Type type);
    }

    public class ActivityStepFactory : IActivityStepFactory
    {
        private readonly ILogger<ActivityStepFactory> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ActivityStepFactory(ILogger<ActivityStepFactory> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public IStep Create<TStep>()
            where TStep : class, IStep
        {
            return Create(typeof(TStep));
        }

        public IStep Create(Type type)
        {
            _logger.LogDebug("Creating activity step of type {StepType}", type.Name);

            if (!typeof(IStep).IsAssignableFrom(type))
            {
                _logger.LogError($"Type {type.FullName} does not implement IActivityStep.");
                throw new ArgumentException($"Type {type.FullName} does not implement IActivityStep.");
            }

            // Use Activator to create an instance of the step
            return (IStep)ActivatorUtilities.CreateInstance(_serviceProvider, type);
        }
    }
}