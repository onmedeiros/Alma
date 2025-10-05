using Alma.Flows.Core.Description.Descriptors;

namespace Alma.Flows.Core.Abstractions
{
    public interface IDataContainingDescriptor
    {
        ICollection<DataDescriptor> DataProperties { get; set; }
    }
}
