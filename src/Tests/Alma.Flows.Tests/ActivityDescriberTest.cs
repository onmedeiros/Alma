using Alma.Flows.Core.Activities.Attributes;
using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Description.Describers;

namespace Alma.Flows.Tests
{
    public class ActivityDescriberTest
    {
        [Activity(Namespace = "TestNamespace", Category = "TestCategory", DisplayName = "Test Activity")]
        private class TestActivity : Activity
        {
            [ActivityParameter(DisplayName = "Test Input")]
            public Parameter<string> TestInput { get; set; } = new Parameter<string>();
        }

        [Test]
        public void Describe_ShouldReturnCorrectActivityDescriptor()
        {
            // Arrange
            var activityType = typeof(TestActivity);
            var expectedTypeName = activityType.FullName;
            var expectedNamespace = "TestNamespace";
            var expectedCategory = "TestCategory";
            var expectedName = nameof(TestActivity);
            var expectedDisplayName = "Test Activity";

            // Act
            var descriptor = ActivityDescriber.Describe(activityType);

            // Assert
            Assert.That(descriptor.TypeName, Is.EqualTo(expectedTypeName));
            Assert.That(descriptor.Namespace, Is.EqualTo(expectedNamespace));
            Assert.That(descriptor.Category, Is.EqualTo(expectedCategory));
            Assert.That(descriptor.Name, Is.EqualTo(expectedName));
            Assert.That(descriptor.DisplayName, Is.EqualTo(expectedDisplayName));
            Assert.That(descriptor.Parameters.Count(), Is.EqualTo(1));
            var inputDescriptor = descriptor.Parameters.First();
            Assert.That(inputDescriptor.Name, Is.EqualTo("TestInput"));
            Assert.That(inputDescriptor.DisplayName, Is.EqualTo("Test Input"));
            Assert.That(inputDescriptor.ValueType, Is.EqualTo("String"));
        }

        [Test]
        public void Describe_ShouldThrowArgumentNullExceptionForNullActivityType()
        {
            // Act & Assert
            //Assert.Throws<ArgumentNullException>(() => ActivityDescriber.Describe(null));
        }

        [Test]
        public void Describe_ShouldThrowExceptionForNonActivityType()
        {
            // Arrange
            var nonActivityType = typeof(string);

            // Act & Assert
            Assert.Throws<Exception>(() => ActivityDescriber.Describe(nonActivityType), "Type must be of type Activity");
        }
    }
}