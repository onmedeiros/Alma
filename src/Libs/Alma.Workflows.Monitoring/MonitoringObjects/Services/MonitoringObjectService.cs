using Alma.Core.Data;
using Alma.Core.Types;
using Alma.Workflows.Monitoring.MonitoringObjects.Entities;
using Alma.Workflows.Monitoring.MonitoringObjectSchemas.Services;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Monitoring.MonitoringObjects.Services
{
    public interface IMonitoringObjectService
    {
        ValueTask<ServiceResult<MonitoringObject>> CreateAsync(MonitoringObject model);

        ValueTask<ServiceResult<MonitoringObject>> ReplaceAsync(MonitoringObject model);

        ValueTask<MonitoringObject?> GetOneAsync(string schemaId, string primaryKey, string? organizationId);

        ValueTask<bool> ExistsAsync(string schemaId, string primaryKey, string? organizationId);
    }

    public class MonitoringObjectService : IMonitoringObjectService
    {
        private readonly ILogger<MonitoringObjectService> _logger;
        private readonly IRepository<MonitoringObject> _repository;
        private readonly IMonitoringObjectSchemaService _monitoringObjectSchemaService;

        public MonitoringObjectService(ILogger<MonitoringObjectService> logger, IRepository<MonitoringObject> repository, IMonitoringObjectSchemaService monitoringObjectSchemaService)
        {
            _logger = logger;
            _repository = repository;
            _monitoringObjectSchemaService = monitoringObjectSchemaService;
        }

        #region Operations

        public async ValueTask<ServiceResult<MonitoringObject>> CreateAsync(MonitoringObject model)
        {
            #region Validation

            var validationResult = await Validate(model);

            if (!validationResult.Succeeded)
                return validationResult;

            #endregion

            await _repository.InsertAsync(model);
            return ServiceResult<MonitoringObject>.Success(model);
        }

        public async ValueTask<ServiceResult<MonitoringObject>> ReplaceAsync(MonitoringObject model)
        {
            #region Validation

            var validationResult = await Validate(model, true);

            if (!validationResult.Succeeded)
                return validationResult;

            #endregion

            var existingObject = await GetOneAsync(model.SchemaId, model.PrimaryKey, model.OrganizationId);

            if (existingObject is null)
            {
                return ServiceResult<MonitoringObject>.OperationError(string.Empty, "Monitoring object not found.");
            }

            existingObject.Data = model.Data;
            await _repository.UpdateAsync(existingObject);

            return ServiceResult<MonitoringObject>.Success(model);
        }

        #endregion

        #region Queries

        public async ValueTask<MonitoringObject?> GetOneAsync(string schemaId, string primaryKey, string? organizationId)
        {
            return await _repository.GetOneAsync(x =>
                x.SchemaId == schemaId
                && x.OrganizationId == organizationId
                && x.PrimaryKey == primaryKey);
        }

        public async ValueTask<bool> ExistsAsync(string schemaId, string primaryKey, string? organizationId)
        {
            return await _repository.ExistsAsync(x =>
                x.SchemaId == schemaId
                && x.OrganizationId == organizationId
                && x.PrimaryKey == primaryKey);
        }

        #endregion

        #region Private

        private async ValueTask<ServiceResult<MonitoringObject>> Validate(MonitoringObject entity, bool ignorePrimaryKeyDuplicate = false)
        {
            var schema = await _monitoringObjectSchemaService.GetOneAsync(entity.SchemaId, entity.OrganizationId);

            if (schema == null)
                return ServiceResult<MonitoringObject>.OperationError(string.Empty, $"Schema with id {entity.SchemaId} not found.");

            // Validate fields against schema
            var serviceErrors = new List<ServiceError>();

            foreach (var field in schema.Fields)
            {
                // Validate Required Field
                if (field.IsRequired && !entity.Data.ContainsKey(field.Name))
                {
                    serviceErrors.Add(new ServiceError
                    {
                        Code = "FieldRequired",
                        Message = $"Field '{field.Name}' is required.",
                        Data = new Dictionary<string, object>
                        {
                            { "FieldName", field.Name }
                        }
                    });

                    continue;
                }

                // Validate Primary Key Field
                if (field.IsPrimaryKey && !entity.Data.ContainsKey(field.Name))
                {
                    serviceErrors.Add(new ServiceError
                    {
                        Code = "FieldPrimaryKey",
                        Message = $"Primary key field '{field.Name}' is required.",
                        Data = new Dictionary<string, object>
                        {
                            { "FieldName", field.Name }
                        }
                    });

                    continue;
                }

                if (!ignorePrimaryKeyDuplicate && field.IsPrimaryKey && entity.Data.ContainsKey(field.Name))
                {
                    var primaryKeyValue = entity.Data[field.Name]!.ToString();
                    var existingObject = await _repository.GetOneAsync(x =>
                        x.SchemaId == entity.SchemaId
                        && x.OrganizationId == entity.OrganizationId
                        && x.PrimaryKey == primaryKeyValue);

                    if (existingObject != null)
                    {
                        serviceErrors.Add(new ServiceError
                        {
                            Code = "FieldPrimaryKeyDuplicate",
                            Message = $"Primary key field '{field.Name}' with value '{primaryKeyValue}' must be unique.",
                            Data = new Dictionary<string, object>
                            {
                                { "FieldName", field.Name },
                                { "FieldValue", primaryKeyValue ?? string.Empty }
                            }
                        });

                        continue;
                    }
                }
            }

            if (serviceErrors.Any())
                return ServiceResult<MonitoringObject>.ValidationError(serviceErrors);

            return ServiceResult<MonitoringObject>.Success(entity);
        }

        #endregion
    }
}