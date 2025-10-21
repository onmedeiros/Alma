using Alma.Workflows.Activities;
using Alma.Workflows.Builders;
using Alma.Workflows.Core.Activities.Steps;
using Alma.Workflows.Enums;
using Alma.Workflows.Registries;
using Alma.Workflows.Runners;
using Alma.Workflows.Tests.Activities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Tests
{
    public class ActivityTest
    {
        private IServiceProvider _serviceProvider;
        private IActivityRegistry _activityRegistry;
        private IActivityRunnerFactory _activityRunnerFactory;

        [SetUp]
        public void Setup()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddAlmaWorkflows(options =>
            {
                options.AddActivity<StartActivity>();
                options.AddActivity<TestActivity>();
            });

            serviceCollection.AddLogging(builder => builder.AddConsole());

            _serviceProvider = serviceCollection.BuildServiceProvider();
            _activityRegistry = _serviceProvider.GetRequiredService<IActivityRegistry>();
            _activityRunnerFactory = _serviceProvider.GetRequiredService<IActivityRunnerFactory>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        [Test]
        public async Task RunActivity_Success()
        {
            // Arrange
            var activityBuilder = _serviceProvider.GetRequiredService<IActivityBuilder<TestActivity>>();

            var activity = activityBuilder.Begin("Activity ID")
                .WithParameter(x => x.Message, "Hello, World!")
                .Build();

            var runner = _activityRunnerFactory.Create(activity);

            // Act
            var result = await runner.ExecuteAsync();

            // Assert
            Assert.That(result.ExecutionStatus, Is.EqualTo(ActivityExecutionStatus.Completed), "Activity should complete successfully.");
        }

        [Test]
        public async Task RunActivityWithSetParametersStep_Success()
        {
            // Arrange
            var activityBuilder = _serviceProvider.GetRequiredService<IActivityBuilder<TestActivity>>();

            var activity = activityBuilder.Begin("Activity ID")
                .WithParameter(x => x.Message, "Hello, World!")
                .WithBeforeExecutionStep<SetParametersStep>()
                .Build();

            var runner = _activityRunnerFactory.Create(activity);

            // Act
            var result = await runner.ExecuteAsync();
        }
    }
}