using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;

namespace Alma.Workflows.Extensions
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