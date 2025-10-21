using Alma.Workflows.Design.Components.Parameters;

namespace Alma.Workflows.Design.Registries
{
    public class ParameterEditorRegistryOptions
    {
        private Dictionary<string, Type> _components = new Dictionary<string, Type>();

        public IReadOnlyDictionary<string, Type> Components => _components;

        public void Register(string name, Type type)
        {
            _components.Add(name, type);
        }

        public void Register<TParameter, TComponent>()
            where TComponent : ParameterEditor
        {
            var name = typeof(TParameter).FullName;

            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Parameter type name is empty.");

            if (_components.ContainsKey(name))
                throw new InvalidOperationException($"Parameter editor for {name} already registered.");

            Register(name, typeof(TComponent));
        }
    }
}