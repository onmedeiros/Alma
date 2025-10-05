using Alma.Flows.Core.Instances.Entities;
using Alma.Core.Types;

namespace Alma.Flows.Core.Instances.Stores
{
    public interface IFlowInstanceStore
    {
        ValueTask<FlowInstance> InsertAsync(FlowInstance instance, CancellationToken cancellationToken = default);
        ValueTask<FlowInstance> UpdateAsync(FlowInstance instance, CancellationToken cancellationToken = default);
        ValueTask<FlowInstance?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);
        ValueTask<FlowInstance?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);
        ValueTask<PagedList<FlowInstance>> ListAsync(int page, int pageSize, FlowInstanceFilters? filters = null, CancellationToken cancellationToken = default);
        ValueTask<string> GetName(string id, string? discriminator = null, CancellationToken cancellationToken = default);
    }

    public class FlowInstanceStore : IFlowInstanceStore
    {
        public ValueTask<FlowInstance> InsertAsync(FlowInstance instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<FlowInstance?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<FlowInstance?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<FlowInstance> UpdateAsync(FlowInstance instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<PagedList<FlowInstance>> ListAsync(int page, int pageSize, FlowInstanceFilters? filters = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<string> GetName(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
