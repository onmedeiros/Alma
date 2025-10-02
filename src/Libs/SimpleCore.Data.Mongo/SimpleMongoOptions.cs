using System.Reflection;

namespace SimpleCore.Data.Mongo
{
    public class SimpleMongoOptions
    {
        public required string ConnectionString { get; set; }
        public required string Database { get; set; }
        public List<Assembly> IndexAssemblies { get; set; } = [];

        public void AddIndexAssembly(Assembly assembly)
        {
            IndexAssemblies.Add(assembly);
        }
    }
}
