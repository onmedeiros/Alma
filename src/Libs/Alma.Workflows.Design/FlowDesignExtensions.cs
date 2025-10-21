using Alma.Workflows.Design.Registries;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows.Design
{
    public static class FlowDesignExtensions
    {
        public static void AddFlowDesign(this IServiceCollection services)
        {
            services.AddTransient<FlowDesignContext>();
            services.AddParameterEditorRegistry();
        }
    }
}