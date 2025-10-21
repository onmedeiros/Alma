using Alma.Workflows.Definitions;
using Alma.Workflows.Stores.Filters;
using Alma.Core.Types;

namespace Alma.Workflows.Stores
{
    public interface IFlowDefinitionStore
    {
        ValueTask<FlowDefinition> InsertAsync(FlowDefinition definition, CancellationToken cancellationToken = default);
        ValueTask<FlowDefinition> UpdateAsync(FlowDefinition definition, CancellationToken cancellationToken = default);
        ValueTask<FlowDefinition?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);
        ValueTask<FlowDefinition?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);
        ValueTask<PagedList<FlowDefinition>> ListAsync(int page, int pageSize, FlowDefinitionFilters? filters = null, CancellationToken cancellationToken = default);
        ValueTask<string> GetName(string id, string? discriminator = null, CancellationToken cancellationToken = default);
    }

    public class FlowDefinitionStore : IFlowDefinitionStore
    {
        public ICollection<FlowDefinition> Collection { get; } = [];

        public ValueTask<FlowDefinition> InsertAsync(FlowDefinition definition, CancellationToken cancellationToken = default)
        {
            Collection.Add(definition);
            return new ValueTask<FlowDefinition>(definition);
        }

        public ValueTask<FlowDefinition?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var query = Collection.AsQueryable();

            if (!string.IsNullOrWhiteSpace(discriminator))
                query = query.Where(x => x.Discriminator == discriminator);

            var definition = query.FirstOrDefault(x => x.Id == id);

            if (definition is not null)
                Collection.Remove(definition);

            return new ValueTask<FlowDefinition?>(definition);
        }

        public ValueTask<FlowDefinition?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var query = Collection.AsQueryable();

            if (!string.IsNullOrWhiteSpace(discriminator))
                query = query.Where(x => x.Discriminator == discriminator);

            return new ValueTask<FlowDefinition?>(query.FirstOrDefault(x => x.Id == id));
        }

        public ValueTask<FlowDefinition> UpdateAsync(FlowDefinition definition, CancellationToken cancellationToken = default)
        {
            var existing = Collection.FirstOrDefault(x => x.Id == definition.Id && x.Discriminator == definition.Discriminator);

            if (existing is not null)
            {
                Collection.Remove(existing);
                Collection.Add(definition);
            }

            return new ValueTask<FlowDefinition>(definition);
        }

        public ValueTask<PagedList<FlowDefinition>> ListAsync(int page, int pageSize, FlowDefinitionFilters? filters = null, CancellationToken cancellationToken = default)
        {
            var query = Collection.AsQueryable();

            if (filters is not null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Name))
                    query = query.Where(x => x.Name.Contains(filters.Name));

                if (!string.IsNullOrWhiteSpace(filters.Discriminator))
                    query = query.Where(x => x.Discriminator == filters.Discriminator);
            }

            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var list = new PagedList<FlowDefinition>
            {
                PageIndex = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            list.AddRange(items);

            return new ValueTask<PagedList<FlowDefinition>>(list);
        }

        public ValueTask<string> GetName(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
