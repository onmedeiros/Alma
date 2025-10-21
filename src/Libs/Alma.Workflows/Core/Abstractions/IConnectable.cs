using Alma.Workflows.Core.Activities.Base;
using System.Reflection;

namespace Alma.Workflows.Core.Abstractions
{
    public interface IConnectable
    {
        IEnumerable<Port> GetPorts();

        IEnumerable<PropertyInfo> GetPortProperties();

        PropertyInfo GetPortProperty(string name);

        void SetPortProperty(string name, Port value);

        void SetPortData(string name, object? value);
    }
}