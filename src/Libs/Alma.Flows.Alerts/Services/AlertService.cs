using Alma.Core.Data;
using Alma.Core.Types;
using Alma.Flows.Alerts.Common;
using Alma.Flows.Alerts.Entities;
using Alma.Flows.Alerts.Stores.Filters;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Alma.Flows.Alerts.Services
{
    public interface IAlertService
    {
        ValueTask<ServiceResult<Alert>> Create(AlertSeverity severity, string title, string? details = null, string? organizationId = null);
        ValueTask<PagedList<Alert>> ListAsync(int page, int pageSize, AlertFilters filters);
        ValueTask<Alert?> FindByIdAsync(string id, string? organizationId = null);
    }

    public class AlertService : IAlertService
    {
        private readonly ILogger<AlertService> _logger;
        private readonly IRepository<Alert> _repository;

        public AlertService(ILogger<AlertService> logger, IRepository<Alert> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async ValueTask<ServiceResult<Alert>> Create(AlertSeverity severity, string title, string? details = null, string? organizationId = null)
        {
            var alert = new Alert
            {
                OrganizationId = organizationId,
                Severity = severity,
                Title = title,
                Details = details
            };

            await _repository.InsertAsync(alert);

            return ServiceResult<Alert>.Success(alert);
        }

        public async ValueTask<PagedList<Alert>> ListAsync(int page, int pageSize, AlertFilters filters)
        {
            var query = _repository.AsQueryable();

            if (!string.IsNullOrEmpty(filters.OrganizationId))
                query = query.Where(x => x.OrganizationId == filters.OrganizationId);

            if (!string.IsNullOrWhiteSpace(filters.Title))
                query = query.Where(x => x.Title.Contains(filters.Title));

            if (filters.Severity.HasValue)
                query = query.Where(x => x.Severity == filters.Severity.Value);

            query = query.OrderByDescending(x => x.CreatedAt);

            return await _repository.GetPagedAsync(page, pageSize, query);
        }

        public async ValueTask<Alert?> FindByIdAsync(string id, string? organizationId = null)
        {
            if (string.IsNullOrEmpty(organizationId))
                return await _repository.GetByIdAsync(id);

            return await _repository.GetOneAsync(x => x.Id == id && x.OrganizationId == organizationId);
        }
    }
}