using System.Reflection;

namespace Alma.Workflows.Core.Abstractions
{
    public interface IDataContaining
    {
        PropertyInfo GetDataProperty(string name);

        object? GetDataValue(string name);

        void SetDataValue(string name, object? value);
    }
}