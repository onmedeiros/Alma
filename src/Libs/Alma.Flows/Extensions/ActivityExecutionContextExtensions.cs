using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Contexts;

namespace Alma.Flows.Extensions
{
    public static class ActivityExecutionContextExtensions
    {
        public static string? Evaluate(this FlowExecutionContext context, Parameter<string>? parameter)
        {
            if (parameter == null)
                return null;

            return parameter.GetValue(context);
        }
    }
}