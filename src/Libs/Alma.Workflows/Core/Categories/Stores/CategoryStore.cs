using Alma.Workflows.Core.Categories.Entities;
using Microsoft.Extensions.Logging;
using Alma.Core.Types;

namespace Alma.Workflows.Core.Categories.Stores
{
    public interface ICategoryStore
    {
        ValueTask<Category> InsertAsync(Category category, CancellationToken cancellationToken = default);

        ValueTask<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default);

        ValueTask<Category?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);

        ValueTask<Category?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);

        ValueTask<PagedList<Category>> ListAsync(int page, int pageSize, CategoryFilters? filters = null, CancellationToken cancellationToken = default);
    }

    public class CategoryStore : ICategoryStore
    {
        private readonly ILogger<CategoryStore> _logger;

        public CategoryStore(ILogger<CategoryStore> logger)
        {
            _logger = logger;
        }

        public ValueTask<Category> InsertAsync(Category category, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Category?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Category?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<PagedList<Category>> ListAsync(int page, int pageSize, CategoryFilters? filters = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}