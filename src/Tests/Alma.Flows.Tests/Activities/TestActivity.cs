using Alma.Flows.Core.Activities.Attributes;
using Alma.Flows.Core.Activities.Base;

namespace Alma.Flows.Tests.Activities
{
    [Activity("Alma.Tests", "Test")]
    internal class TestActivity : Activity
    {
        [ActivityParameter("Mensagem")]
        public Parameter<string>? Message { get; set; }

        public string? InvalidInput { get; set; }
    }
}