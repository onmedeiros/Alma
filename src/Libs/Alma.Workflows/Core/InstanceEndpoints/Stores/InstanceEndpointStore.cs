using Alma.Workflows.Core.Common.Enums;
using Alma.Workflows.Core.InstanceEndpoints.Entities;
using Alma.Core.Types;

namespace Alma.Workflows.Core.InstanceEndpoints.Stores
{
    public interface IInstanceEndpointStore
    {
        ValueTask<InstanceEndpoint> InsertAsync(InstanceEndpoint instance, CancellationToken cancellationToken = default);

        ValueTask<InstanceEndpoint?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);

        ValueTask<InstanceEndpoint> UpdateAsync(InstanceEndpoint instance, CancellationToken cancellationToken = default);

        ValueTask<InstanceEndpoint?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);

        ValueTask<InstanceEndpoint?> FindByPathAsync(string apiId, string path, ApiMethod method, string? discriminator = null, CancellationToken cancellationToken = default);

        ValueTask<PagedList<InstanceEndpoint>> ListAsync(int page, int pageSize, InstanceEndpointFilters? filters = null, CancellationToken cancellationToken = default);
    }

    public class InstanceEndpointStore : IInstanceEndpointStore
    {
        public ValueTask<InstanceEndpoint> InsertAsync(InstanceEndpoint instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<InstanceEndpoint?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<InstanceEndpoint> UpdateAsync(InstanceEndpoint instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<InstanceEndpoint?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<InstanceEndpoint?> FindByPathAsync(string apiId, string path, ApiMethod method, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<PagedList<InstanceEndpoint>> ListAsync(int page, int pageSize, InstanceEndpointFilters? filters = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}