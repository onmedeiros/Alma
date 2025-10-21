using Alma.Workflows.Definitions;
using Alma.Workflows.Stores.Filters;
using Alma.Core.Types;

namespace Alma.Workflows.Stores
{
    public interface IFlowDefinitionVersionStore
    {
        ValueTask<FlowDefinitionVersion> InsertAsync(FlowDefinitionVersion instance, CancellationToken cancellationToken = default);
        ValueTask<FlowDefinitionVersion?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);
        ValueTask<PagedList<FlowDefinitionVersion>> ListAsync(int page, int pageSize, FlowDefinitionVersionFilters? filters = null, CancellationToken cancellationToken = default);
        ValueTask<FlowDefinitionVersion?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);
        ValueTask<string> GetName(string id, string? discriminator = null, CancellationToken cancellationToken = default);

    }
    public class FlowDefinitionVersionStore : IFlowDefinitionVersionStore
    {
        public ValueTask<FlowDefinitionVersion> InsertAsync(FlowDefinitionVersion instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<FlowDefinitionVersion?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<PagedList<FlowDefinitionVersion>> ListAsync(int page, int pageSize, FlowDefinitionVersionFilters? filters = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<FlowDefinitionVersion?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<string> GetName(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
