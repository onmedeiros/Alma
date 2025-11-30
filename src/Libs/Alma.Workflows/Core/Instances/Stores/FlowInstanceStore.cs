using Alma.Workflows.Core.Instances.Entities;
using Alma.Core.Types;

namespace Alma.Workflows.Core.Instances.Stores
{
    public interface IFlowInstanceStore
    {
        ValueTask<Instance> InsertAsync(Instance instance, CancellationToken cancellationToken = default);

        ValueTask<Instance> UpdateAsync(Instance instance, CancellationToken cancellationToken = default);

        ValueTask<Instance?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);

        ValueTask<Instance?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);

        ValueTask<PagedList<Instance>> ListAsync(int page, int pageSize, FlowInstanceFilters? filters = null, CancellationToken cancellationToken = default);

        ValueTask<string> GetName(string id, string? discriminator = null, CancellationToken cancellationToken = default);

        ValueTask<int> CountAsync(FlowInstanceFilters? filters = null, CancellationToken cancellationToken = default);
    }

    public class FlowInstanceStore : IFlowInstanceStore
    {
        public ValueTask<Instance> InsertAsync(Instance instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Instance?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Instance?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Instance> UpdateAsync(Instance instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<PagedList<Instance>> ListAsync(int page, int pageSize, FlowInstanceFilters? filters = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<string> GetName(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<int> CountAsync(FlowInstanceFilters? filters = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}