using Alma.Flows.Core.CustomActivities.Entities;
using Alma.Core.Types;

namespace Alma.Flows.Core.CustomActivities.Stores
{
    public interface ICustomActivityTemplateStore
    {
        ValueTask<CustomActivityTemplate> InsertAsync(CustomActivityTemplate activity, CancellationToken cancellationToken = default);

        ValueTask<CustomActivityTemplate?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);

        ValueTask<CustomActivityTemplate?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);

        ValueTask<CustomActivityTemplate> UpdateAsync(CustomActivityTemplate activity, CancellationToken cancellationToken = default);

        ValueTask<PagedList<CustomActivityTemplate>> ListAsync(int page, int pageSize, CustomActivityFilters? filters = null, CancellationToken cancellationToken = default);

        ValueTask<CustomActivityScript> InsertScriptAsync(CustomActivityScript script, CancellationToken cancellationToken = default);

        ValueTask<CustomActivityScript?> FindScriptAsync(string customActivityId, string? discriminator = null, CancellationToken cancellationToken = default);

        ValueTask<CustomActivityScript> UpdateScriptAsync(CustomActivityScript script, CancellationToken cancellationToken = default);
    }

    public class CustomActivityStore : ICustomActivityTemplateStore
    {
        public ValueTask<CustomActivityTemplate> InsertAsync(CustomActivityTemplate activity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<CustomActivityTemplate?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<CustomActivityTemplate?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<CustomActivityTemplate> UpdateAsync(CustomActivityTemplate activity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<PagedList<CustomActivityTemplate>> ListAsync(int page, int pageSize, CustomActivityFilters? filters = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<CustomActivityScript> InsertScriptAsync(CustomActivityScript script, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<CustomActivityScript?> FindScriptAsync(string customActivityId, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<CustomActivityScript> UpdateScriptAsync(CustomActivityScript script, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}