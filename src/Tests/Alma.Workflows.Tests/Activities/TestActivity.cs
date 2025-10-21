using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;

namespace Alma.Workflows.Tests.Activities
{
    [Activity("Alma.Tests", "Test")]
    internal class TestActivity : Activity
    {
        [ActivityParameter("Mensagem")]
        public Parameter<string>? Message { get; set; }

        public string? InvalidInput { get; set; }
    }
}