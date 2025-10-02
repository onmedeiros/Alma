using System.Reflection;

namespace Alma.Core.Modules
{
    public class ModuleOptions
    {
        private ICollection<Assembly> _assemblies = [];
        private ICollection<IModule> _modules = [];

        public IEnumerable<Assembly> Assemblies => _assemblies;
        public IEnumerable<IModule> Modules => _modules;

        public ModuleOptions Register(Assembly assembly)
        {
            _assemblies.Add(assembly);
            return this;
        }

        public ModuleOptions AddModule(IModule module)
        {
            _modules.Add(module);
            return this;
        }
    }
}
