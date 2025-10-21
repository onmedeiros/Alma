using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Description.Descriptors;
using Moq;

namespace Alma.Workflows.Tests.Core.Activities.Base
{
    public class PortTest
    {
        private static Port CreatePort(Type? dataType = null, PortType type = PortType.Input)
        {
            var activityMock = new Mock<IActivity>(MockBehavior.Loose);

            var port = new Port
            {
                Activity = activityMock.Object,
                Descriptor = new PortDescriptor
                {
                    Name = "P1",
                    DisplayName = "Port 1",
                    Type = type,
                    DataType = dataType
                },
                Type = type,
                DataType = dataType
            };

            return port;
        }

        [Test]
        public void Defaults_ShouldBeCorrect()
        {
            var port = CreatePort();

            Assert.That(port.Executed, Is.False);
            Assert.That(port.Type, Is.EqualTo(PortType.Input));
            Assert.That(port.DataType, Is.Null);
            Assert.That(port.Data, Is.Null);
            Assert.That(port.ConnectedPorts, Is.Not.Null);
            Assert.That(port.ConnectedPorts.Count, Is.EqualTo(0));
        }

        [Test]
        public void Execute_ShouldSetExecutedTrue_WithoutData()
        {
            var port = CreatePort();

            port.Execute();

            Assert.That(port.Executed, Is.True);
            Assert.That(port.Data, Is.Null);
        }

        [Test]
        public void Execute_WithData_NoDataTypeConstraint_ShouldStoreDataAndExecute()
        {
            var port = CreatePort(dataType: null);

            port.Execute(123);

            Assert.That(port.Executed, Is.True);
            Assert.That(port.Data, Is.EqualTo(123));
        }

        [Test]
        public void Execute_WithMatchingDataType_ShouldStoreDataAndExecute()
        {
            var port = CreatePort(typeof(string));

            port.Execute("hello");

            Assert.That(port.Executed, Is.True);
            Assert.That(port.Data, Is.EqualTo("hello"));
        }

        [Test]
        public void Execute_WithMismatchedDataType_ShouldThrowAndNotChangeState()
        {
            var port = CreatePort(typeof(string));

            var ex = Assert.Throws<InvalidOperationException>(() => port.Execute(10));
            Assert.That(ex!.Message, Does.Contain("Invalid data type"));
            Assert.That(ex.Message, Does.Contain(typeof(string).ToString()));
            Assert.That(ex.Message, Does.Contain(typeof(int).ToString()));

            Assert.That(port.Executed, Is.False);
            Assert.That(port.Data, Is.Null);
        }

        [Test]
        public void Execute_WithNullForReferenceType_ShouldSucceed()
        {
            var port = CreatePort(typeof(string));

            port.Execute<string>(null);

            Assert.That(port.Executed, Is.True);
            Assert.That(port.Data, Is.Null);
        }
    }
}