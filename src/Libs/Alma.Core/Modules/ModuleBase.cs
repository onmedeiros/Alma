using Microsoft.Extensions.DependencyInjection;

namespace Alma.Core.Modules
{
    public class ModuleBase : IModule
    {
        public virtual ModuleDescriptor Descriptor => throw new NotImplementedException();

        public virtual void Configure(IServiceCollection services)
        {
            // implement this method in the derived class.
        }
    }
}
