using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows
{
    public class AlmaFlowBuilder
    {
        public IServiceCollection Services { get; }

        public AlmaFlowBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
