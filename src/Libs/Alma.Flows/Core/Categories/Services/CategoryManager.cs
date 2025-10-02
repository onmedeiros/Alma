using Alma.Flows.Core.Categories.Base;
using Alma.Flows.Core.Categories.Entities;
using Alma.Flows.Core.Categories.Models;
using Alma.Flows.Core.Categories.Stores;
using Microsoft.Extensions.Logging;
using Alma.Core.Types;

namespace Alma.Flows.Core.Categories.Services
{
    public interface ICategoryManager
    {
        ValueTask<Category> Create(CategoryCreateModel model);

        ValueTask<Category> Update(CategoryUpdateModel model);

        ValueTask<Category?> Delete(string id, string? discriminator = null);

        ValueTask<Category?> FindById(string id, string? discriminator = null);

        ValueTask<PagedList<Category>> List(int page, int pageSize, CategoryFilters? filters = null);
    }

    public class CategoryManager : ICategoryManager
    {
        private readonly ILogger<CategoryManager> _logger;
        private readonly ICategoryStore _categoryStore;

        public CategoryManager(ILogger<CategoryManager> logger, ICategoryStore categoryStore)
        {
            _logger = logger;
            _categoryStore = categoryStore;
        }

        public ValueTask<Category> Create(CategoryCreateModel model)
        {
            _logger.LogDebug("Creating category with name {Name}.", model.DefaultName);

            var now = DateTime.Now;

            var category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Discriminator = model.Discriminator,
                CreatedAt = now,
                UpdatedAt = now,
                DefaultName = model.DefaultName,
                ResourceName = model.ResourceName
            };

            return _categoryStore.InsertAsync(category);
        }

        public async ValueTask<Category> Update(CategoryUpdateModel model)
        {
            _logger.LogDebug("Updating category with id {Id}.", model.Id);

            var entity = await FindById(model.Id, model.Discriminator);

            if (entity == null)
            {
                _logger.LogError("Category with id {Id} not found.", model.Id);
                throw new Exception($"Category with id {model.Id} not found.");
            }

            entity.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrEmpty(model.ResourceName) && model.ResourceName != entity.ResourceName)
                entity.ResourceName = model.ResourceName;

            if (!string.IsNullOrEmpty(model.DefaultName) && model.DefaultName != entity.DefaultName)
                entity.DefaultName = model.DefaultName;

            return await _categoryStore.UpdateAsync(entity);
        }

        public async ValueTask<Category?> Delete(string id, string? discriminator = null)
        {
            _logger.LogDebug("Deleting category with id {Id}.", id);

            var entity = await FindById(id, discriminator);

            if (entity == null)
            {
                _logger.LogError("Category with id {Id} not found.", id);
                throw new Exception($"Category with id {id} not found.");
            }

            return await _categoryStore.DeleteAsync(id, discriminator);
        }

        public ValueTask<Category?> FindById(string id, string? discriminator = null)
        {
            _logger.LogDebug("Finding category with id {Id}.", id);

            return _categoryStore.FindByIdAsync(id, discriminator);
        }

        public async ValueTask<PagedList<Category>> List(int page, int pageSize, CategoryFilters? filters = null)
        {
            var categories = await _categoryStore.ListAsync(page, pageSize, filters);

            categories.AddRange(DefaultCategories.All.Where(x => x.DefaultName.Contains(filters?.Name ?? string.Empty)));

            return categories;
        }
    }
}