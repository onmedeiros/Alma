using DotLiquid;

namespace Alma.Workflows.Tests
{
    public class TemplateTest
    {
        [Test]
        public async Task TemplateResolution()
        {
            // arrange
            var dict = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", new { Value2 = "value2"} }
            };

            var template = Template.Parse("This is {{key1}} and {{key2.Value2}}.");

            // act
            var render = template.Render(Hash.FromDictionary(dict));

            // assert
            Assert.That(render, Is.EqualTo("This is value1 and value2."));
        }
    }
}