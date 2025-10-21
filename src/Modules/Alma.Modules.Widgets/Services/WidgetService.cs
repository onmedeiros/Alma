using Alma.Core.Data;
using Alma.Modules.Widgets.Entities;
using Microsoft.Extensions.Logging;

namespace Alma.Modules.Widgets.Services
{
    public interface IWidgetService
    {
        public Task Create(Widget widget);

        public Task Update(Widget widget);

        public Task Delete(string id, string? organizationId = null);

        public Task<Widget?> GetById(string id, string? organizationId = null);

        public Task<List<Widget>> GetByContainer(string container, string? organizationId = null);
    }

    public class WidgetService : IWidgetService
    {
        private readonly ILogger<WidgetService> _logger;
        private readonly IRepository<Widget> _repository;

        public WidgetService(ILogger<WidgetService> logger, IRepository<Widget> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public Task Create(Widget widget)
        {
            return _repository.InsertAsync(widget);
        }

        public Task Update(Widget widget)
        {
            return _repository.UpdateAsync(widget);
        }

        public Task Delete(string id, string? organizationId = null)
        {
            return _repository.DeleteAsync(id);
        }

        public Task<List<Widget>> GetByContainer(string container, string? organizationId = null)
        {
            var query = _repository.AsQueryable()
                .Where(x => x.Container == container && x.OrganizationId == organizationId);

            return _repository.ToListAsync(query);
        }

        public Task<Widget?> GetById(string id, string? organizationId = null)
        {
            throw new NotImplementedException();
        }
    }
}