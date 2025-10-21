using Alma.Workflows.Core.Description.Descriptors;

namespace Alma.Workflows.Core.Abstractions
{
    public interface IDataContainingDescriptor
    {
        ICollection<DataDescriptor> DataProperties { get; set; }
    }
}
