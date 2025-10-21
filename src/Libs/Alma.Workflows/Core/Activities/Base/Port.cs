using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Description.Descriptors;

namespace Alma.Workflows.Core.Activities.Base
{
    public class Port
    {
        public bool Executed { get; set; }
        public required PortDescriptor Descriptor { get; set; }
        public required IActivity Activity { get; set; }
        public PortType Type { get; set; } = PortType.Input;
        public Type? DataType { get; set; }
        public object? Data { get; set; }

        public ICollection<Port> ConnectedPorts { get; set; } = [];

        public void Execute()
        {
            Executed = true;
        }

        public void Execute<TData>(TData data)
        {
            if (DataType is not null && DataType != typeof(TData))
                throw new InvalidOperationException($"Invalid data type. Expected {DataType}, but got {typeof(TData)}.");

            Data = data;

            Execute();
        }
    }
}