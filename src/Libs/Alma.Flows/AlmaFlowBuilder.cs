using Microsoft.Extensions.DependencyInjection;

namespace Alma.Flows
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
