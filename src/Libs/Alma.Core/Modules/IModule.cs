using Microsoft.Extensions.DependencyInjection;

namespace Alma.Core.Modules
{
    public interface IModule
    {
        ModuleDescriptor Descriptor { get; }
        void Configure(IServiceCollection services);
    }
}
