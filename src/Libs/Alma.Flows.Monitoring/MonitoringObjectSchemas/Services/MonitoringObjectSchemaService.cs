using Alma.Core.Data;
using Alma.Core.Types;
using Alma.Flows.Monitoring.MonitoringObjectSchemas.Entities;
using Alma.Flows.Monitoring.MonitoringObjectSchemas.Models;
using Microsoft.Extensions.Logging;
using Alma.Core.Validations;

namespace Alma.Flows.Monitoring.MonitoringObjectSchemas.Services
{
    public interface IMonitoringObjectSchemaService
    {
        ValueTask<ServiceResult<MonitoringObjectSchema>> CreateAsync(MonitoringObjectSchemaCreateModel model);

        ValueTask<ServiceResult<MonitoringObjectSchema>> UpdateAsync(MonitoringObjectSchemaEditModel model);

        ValueTask<MonitoringObjectSchema?> GetOneAsync(string id, string? organizationId = null);

        ValueTask<PagedList<MonitoringObjectSchema>> SearchAsync(MonitoringObjectSchemaSearchModel model);
    }

    public class MonitoringObjectSchemaService : IMonitoringObjectSchemaService
    {
        private readonly ILogger<MonitoringObjectSchemaService> _logger;
        private readonly IRepository<MonitoringObjectSchema> _repository;
        public readonly IValidator _validator;

        public MonitoringObjectSchemaService(ILogger<MonitoringObjectSchemaService> logger, IRepository<MonitoringObjectSchema> repository, IValidator validator)
        {
            _logger = logger;
            _repository = repository;
            _validator = validator;
        }

        #region Operations

        public async ValueTask<ServiceResult<MonitoringObjectSchema>> CreateAsync(MonitoringObjectSchemaCreateModel model)
        {
            #region Validation

            var validationResult = await _validator.Validate(model);

            if (!validationResult.IsValid)
                return ServiceResult<MonitoringObjectSchema>.ValidationError(validationResult);

            #endregion

            var schema = new MonitoringObjectSchema
            {
                OrganizationId = model.OrganizationId,
                Name = model.Name
            };

            await _repository.InsertAsync(schema);

            return ServiceResult<MonitoringObjectSchema>.Success(schema);
        }

        public async ValueTask<ServiceResult<MonitoringObjectSchema>> UpdateAsync(MonitoringObjectSchemaEditModel model)
        {
            #region Validation

            var validationResult = await _validator.Validate(model);

            if (!validationResult.IsValid)
                return ServiceResult<MonitoringObjectSchema>.ValidationError(validationResult);

            #endregion

            var schema = await GetOneAsync(model.Id, model.OrganizationId);

            if (schema is null)
            {
                _logger.LogWarning("Monitoring Object Schema {SchemaId} not found.", model.Id);
                return ServiceResult<MonitoringObjectSchema>.OperationError(string.Empty, $"Monitoring Object Schema with id {model.Id} not found in Organization {model.OrganizationId}.");
            }

            var update = _repository.BeginUpdate()
                .Where(x => x.Id == model.Id && x.OrganizationId == model.OrganizationId);

            if (!string.IsNullOrWhiteSpace(model.Name) && model.Name != schema.Name)
                update = update.Set(x => x.Name, model.Name);

            if (schema.Fields.Except(model.Fields).Any() || model.Fields.Except(schema.Fields).Any())
                update.Set(x => x.Fields, model.Fields);

            if (!update.HasChanges())
                return ServiceResult<MonitoringObjectSchema>.WarningError("Nenhuma alteração pendente.");

            var updated = await update.ExecuteAsync();

            if (updated is null)
                return ServiceResult<MonitoringObjectSchema>.OperationError(string.Empty, $"Monitoring Object Schema with id {model.Id} in Organization {model.OrganizationId} was not updated.");

            return ServiceResult<MonitoringObjectSchema>.Success(updated);
        }

        #endregion

        #region Queries

        public async ValueTask<MonitoringObjectSchema?> GetOneAsync(string id, string? organizationId = null)
        {
            if (string.IsNullOrEmpty(organizationId))
                return await _repository.GetByIdAsync(id);

            return await _repository.GetOneAsync(x => x.Id == id && x.OrganizationId == organizationId);
        }

        public async ValueTask<PagedList<MonitoringObjectSchema>> SearchAsync(MonitoringObjectSchemaSearchModel model)
        {
            var query = _repository.AsQueryable();

            if (!string.IsNullOrEmpty(model.OrganizationId))
                query = query.Where(x => x.OrganizationId == model.OrganizationId);

            if (!string.IsNullOrEmpty(model.Term))
                query = query.Where(x => x.Name.Contains(model.Term));

            if (!string.IsNullOrEmpty(model.OrderBy))
            {
                var property = model.OrderBy.ToLowerInvariant();
                if (property == "typename")
                {
                    query = query.OrderBy(x => x.Name);
                }
                else if (property == "createdat")
                {
                    query = query.OrderBy(x => x.CreatedAt);
                }
                else
                {
                    _logger.LogWarning("Unknown order by property: {Property}. Defaulting to TypeName.", model.OrderBy);
                    query = query.OrderBy(x => x.Name);
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.Name);
            }

            return await _repository.GetPagedAsync(model.PageIndex, model.PageSize, query);
        }

        #endregion
    }
}