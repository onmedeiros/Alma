using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.CustomActivities.Entities;
using Alma.Workflows.Core.Description.Describers;
using Alma.Workflows.Tests.Activities;

namespace Alma.Workflows.Tests.Core.Descriptions.Describers
{
    public class ActivityDescriberTest
    {
        [Activity("My.Namespace", "MyCategory", DisplayName = "My Activity", Description = "Desc", RequireInteraction = true)]
        private class DescriberTestActivity : Activity
        {
            [ActivityParameter("Param Display")]
            public Parameter<string> P1 { get; set; } = new();

            [Port(DisplayName = "Out Port", Type = PortType.Output, DataType = typeof(int))]
            public Port Out { get; set; }

            public Data<double> Output { get; set; } = new();
        }

        [Test]
        public void Describe_Type_WithAttribute_ShouldPopulateDescriptor()
        {
            var t = typeof(DescriberTestActivity);

            var d = ActivityDescriber.Describe(t);

            Assert.That(d.Type, Is.EqualTo(t));
            Assert.That(d.TypeName, Is.EqualTo(t.FullName));
            Assert.That(d.Namespace, Is.EqualTo("My.Namespace"));
            Assert.That(d.Category, Is.EqualTo("MyCategory"));
            Assert.That(d.Name, Is.EqualTo(nameof(DescriberTestActivity)));
            Assert.That(d.FullName, Is.EqualTo($"My.Namespace.{nameof(DescriberTestActivity)}"));
            Assert.That(d.DisplayName, Is.EqualTo("My Activity"));
            Assert.That(d.Description, Is.EqualTo("Desc"));
            Assert.That(d.RequireInteraction, Is.True);

            // Parameters
            Assert.That(d.Parameters.Count, Is.EqualTo(1));
            var p = d.Parameters.First();
            Assert.That(p.Name, Is.EqualTo("P1"));
            Assert.That(p.DisplayName, Is.EqualTo("Param Display"));
            Assert.That(p.ValueType, Is.EqualTo("String"));
            Assert.That(p.Type, Is.EqualTo(typeof(string)));

            // Ports
            Assert.That(d.Ports.Count, Is.EqualTo(1));
            var port = d.Ports.First();
            Assert.That(port.Name, Is.EqualTo("Out"));
            Assert.That(port.DisplayName, Is.EqualTo("Out Port"));
            Assert.That(port.Type, Is.EqualTo(PortType.Output));
            Assert.That(port.DataType, Is.EqualTo(typeof(int)));
            Assert.That(port.DataTypeName, Is.EqualTo(typeof(int).FullName));

            // Data properties
            Assert.That(d.DataProperties.Count, Is.EqualTo(1));
            var data = d.DataProperties.First();
            Assert.That(data.Name, Is.EqualTo("Output"));
            Assert.That(data.Type, Is.EqualTo(typeof(double)));
        }

        [Test]
        public void Describe_Type_WithoutAttribute_ShouldFallbackToTypeInfo()
        {
            var t = typeof(TestActivity); // from existing tests: [Activity("Alma.Tests", "Test")]

            var d = ActivityDescriber.Describe(t);

            Assert.That(d.Namespace, Is.EqualTo("Alma.Tests"));
            Assert.That(d.Category, Is.EqualTo("Test"));
            Assert.That(d.DisplayName, Is.EqualTo(nameof(TestActivity)));
        }

        [Test]
        public void Describe_Type_Null_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => ActivityDescriber.Describe((Type)null!));
        }

        [Test]
        public void Describe_Type_NotActivity_ShouldThrow()
        {
            Assert.Throws<Exception>(() => ActivityDescriber.Describe(typeof(string)));
        }

        [Test]
        public void Describe_CustomActivityTemplate_ShouldMapFields()
        {
            var template = new CustomActivityTemplate
            {
                Id = "abc",
                Name = "Template Activity",
                Description = "TDesc",
                CategoryId = "cat",
                Parameters =
                [
                    new CustomActivityParameterTemplate{ Id = "p1", Name = "A", DisplayName = "A D", Type = Alma.Workflows.Core.Common.Enums.ParameterType.String },
                    new CustomActivityParameterTemplate{ Id = "p2", Name = "B", DisplayName = "B D", Type = Alma.Workflows.Core.Common.Enums.ParameterType.Int }
                ],
                Ports =
                [
                    new CustomActivityPort{ Id = "o1", Name = "Out", DisplayName = "Out D", Type = PortType.Output }
                ]
            };

            var d = ActivityDescriber.Describe(template);

            var customType = typeof(Alma.Workflows.Core.CustomActivities.CustomActivity);

            Assert.That(d.Type, Is.EqualTo(customType));
            Assert.That(d.TypeName, Is.EqualTo($"{customType.FullName}+{template.Id}"));
            Assert.That(d.FullName, Is.EqualTo($"{customType.FullName}+{template.Id}"));
            Assert.That(d.Name, Is.EqualTo($"{customType.Name}+{template.Id}"));
            Assert.That(d.DisplayName, Is.EqualTo("Template Activity"));
            Assert.That(d.Description, Is.EqualTo("TDesc"));
            Assert.That(d.Category, Is.EqualTo("cat"));

            Assert.That(d.Parameters.Count, Is.EqualTo(2));
            Assert.That(d.Parameters.Any(p => p.Name == "A" && p.DisplayName == "A D" && p.ValueType == "String"));
            Assert.That(d.Parameters.Any(p => p.Name == "B" && p.DisplayName == "B D" && p.ValueType == "Int32"));

            Assert.That(d.Ports.Count, Is.EqualTo(1));
            var port = d.Ports.First();
            Assert.That(port.Name, Is.EqualTo("Out"));
            Assert.That(port.DisplayName, Is.EqualTo("Out D"));
            Assert.That(port.Type, Is.EqualTo(PortType.Output));
        }
    }
}
