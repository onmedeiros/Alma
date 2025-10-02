using Alma.Flows.Core.Description.Describers;
using Alma.Flows.Tests.Activities;

namespace Alma.Flows.Tests
{
    public class InputDescriberTests
    {
        [Test]
        public void Describe_ShouldReturnCorrectInputDescriptor()
        {
            // Arrange
            var property = typeof(TestActivity).GetProperty(nameof(TestActivity.Message));
            var expectedName = nameof(TestActivity.Message);
            var expectedDisplayName = "Mensagem";
            var expectedValueType = "String";
            var expectedType = typeof(string);

            // Act
            var descriptor = ParameterDescriber.Describe(property);

            // Assert
            Assert.That(expectedName == descriptor.Name);
            Assert.That(expectedDisplayName == descriptor.DisplayName);
            Assert.That(expectedValueType == descriptor.ValueType);
            Assert.That(expectedType == descriptor.Type);
        }

        [Test]
        public void Describe_ShouldThrowExceptionForInvalidPropertyType()
        {
            // Arrange
            var property = typeof(TestActivity).GetProperty(nameof(TestActivity.InvalidInput));

            // Act & Assert
            Assert.Throws<Exception>(() => ParameterDescriber.Describe(property), "Property must be of type Input<T>");
        }
    }
}