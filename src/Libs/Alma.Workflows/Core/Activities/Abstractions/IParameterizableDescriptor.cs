using Alma.Workflows.Core.Description.Descriptors;

namespace Alma.Workflows.Core.Activities.Abstractions
{
    public interface IParameterizableDescriptor
    {
        public ICollection<ParameterDescriptor> Parameters { get; set; }
    }
}
