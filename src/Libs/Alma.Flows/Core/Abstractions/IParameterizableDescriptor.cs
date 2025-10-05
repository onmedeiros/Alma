using Alma.Flows.Core.Description.Descriptors;

namespace Alma.Flows.Core.Abstractions
{
    public interface IParameterizableDescriptor
    {
        public ICollection<ParameterDescriptor> Parameters { get; set; }
    }
}
