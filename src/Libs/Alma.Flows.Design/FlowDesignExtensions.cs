using Alma.Flows.Design.Registries;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Flows.Design
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