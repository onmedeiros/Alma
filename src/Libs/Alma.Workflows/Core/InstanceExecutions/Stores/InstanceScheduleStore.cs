using Alma.Workflows.Core.InstanceExecutions.Entities;
using Alma.Core.Types;

namespace Alma.Workflows.Core.InstanceExecutions.Stores
{
    public interface IInstanceExecutionStore
    {
        ValueTask<InstanceExecution> InsertAsync(InstanceExecution instance, CancellationToken cancellationToken = default);
        ValueTask<InstanceExecution> UpdateAsync(InstanceExecution instance, CancellationToken cancellationToken = default);
        ValueTask<InstanceExecution?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);
        ValueTask<InstanceExecution?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);
        ValueTask<PagedList<InstanceExecution>> ListAsync(int page, int pageSize, InstanceExecutionFilters? filters = null, CancellationToken cancellationToken = default);
    }

    public class InstanceExecutionStore : IInstanceExecutionStore
    {
        public ValueTask<InstanceExecution> InsertAsync(InstanceExecution instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<InstanceExecution?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<InstanceExecution?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<InstanceExecution> UpdateAsync(InstanceExecution instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<PagedList<InstanceExecution>> ListAsync(int page, int pageSize, InstanceExecutionFilters? filters = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
