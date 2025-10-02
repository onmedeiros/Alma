using Alma.Core.Data;
using Alma.Core.Types;
using Alma.Integrations.Apis.Entities;
using Alma.Integrations.Apis.Models;
using Alma.Integrations.Apis.Validators;
using Microsoft.Extensions.Logging;
using Alma.Core.Validations;

namespace Alma.Integrations.Apis.Services
{
    public interface IApiService
    {
        ValueTask<ServiceResult<Api>> CreateAsync(ApiCreateModel model);

        ValueTask<ServiceResult<Api>> UpdateAsync(ApiEditModel model);

        ValueTask<bool> ExistsAsync(string id, string organizationId);

        ValueTask<Api?> GetOneAsync(string id, string organizationId);

        ValueTask<Api?> GetByPathAsync(string path, string organizationId);

        ValueTask<PagedList<Api>> SearchAsync(ApiSearchModel model);
    }

    public class ApiService : IApiService
    {
        private readonly ILogger<ApiService> _logger;
        private readonly IRepository<Api> _repository;
        private readonly IValidator _validator;

        public ApiService(ILogger<ApiService> logger, IRepository<Api> repository, IValidator validator)
        {
            _logger = logger;
            _repository = repository;
            _validator = validator;
        }

        public async ValueTask<ServiceResult<Api>> CreateAsync(ApiCreateModel model)
        {
            #region Validation

            var validationResult = await _validator.Validate(model);

            if (!validationResult.IsValid)
                return ServiceResult<Api>.ValidationError(validationResult);

            #endregion

            var api = new Api
            {
                OrganizationId = model.OrganizationId,
                Name = model.Name,
                Path = model.Path.Trim().TrimStart('/').TrimEnd('/').ToLowerInvariant(),
                IsActive = model.IsActive,
            };

            await _repository.InsertAsync(api);
            return ServiceResult<Api>.Success(api);
        }

        public async ValueTask<ServiceResult<Api>> UpdateAsync(ApiEditModel model)
        {
            #region Validation

            var validationResult = await _validator.Validate(model);

            if (!validationResult.IsValid)
                return ServiceResult<Api>.ValidationError(validationResult);

            #endregion

            var api = await GetOneAsync(model.Id, model.OrganizationId);

            if (api is null)
            {
                _logger.LogWarning("API with id {Id} not found in organization {OrganizationId}.", model.Id, model.OrganizationId);
                return ServiceResult<Api>.OperationError(string.Empty, $"API with id {model.Id} not found in Organization {model.OrganizationId}.");
            }

            var update = _repository.BeginUpdate()
                .Where(x => x.Id == model.Id && x.OrganizationId == model.OrganizationId);

            if (!string.IsNullOrEmpty(model.Name) && model.Name != api.Name)
                update.Set(x => x.Name, model.Name);

            if (!string.IsNullOrEmpty(model.Path) && model.Path != api.Path)
            {
                model.Path = model.Path.Trim().TrimStart('/').TrimEnd('/').ToLowerInvariant();
                update.Set(x => x.Path, model.Path);
            }

            if (model.IsActive != api.IsActive)
                update.Set(x => x.IsActive, model.IsActive);

            if (!update.HasChanges())
                return ServiceResult<Api>.WarningError("Nenhuma alteração pendente.");

            var updatedApi = await update.ExecuteAsync();

            if (updatedApi is null)
                return ServiceResult<Api>.OperationError(string.Empty, $"API with id {model.Id} in Organization {model.OrganizationId} was not updated.");

            return ServiceResult<Api>.Success(updatedApi);
        }

        public async ValueTask<bool> ExistsAsync(string id, string organizationId)
        {
            return await _repository.ExistsAsync(x => x.Id == id && x.OrganizationId == organizationId);
        }

        public async ValueTask<Api?> GetOneAsync(string id, string organizationId)
        {
            return await _repository.GetOneAsync(x => x.Id == id && x.OrganizationId == organizationId);
        }

        public async ValueTask<Api?> GetByPathAsync(string path, string organizationId)
        {
            path = path.Trim().TrimStart('/').TrimEnd('/').ToLowerInvariant();
            return await _repository.GetOneAsync(x => x.Path == path && x.OrganizationId == organizationId);
        }

        public async ValueTask<PagedList<Api>> SearchAsync(ApiSearchModel model)
        {
            var query = _repository.AsQueryable();

            query = query.Where(x => x.OrganizationId == model.OrganizationId);

            if (!string.IsNullOrEmpty(model.Term))
                query = query.Where(x => x.Name.Contains(model.Term));

            if (!string.IsNullOrEmpty(model.OrderBy))
            {
                var property = model.OrderBy.ToLowerInvariant();

                if (property == "name")
                {
                    query = query.OrderBy(x => x.Name);
                }
                else if (property == "createdat")
                {
                    query = query.OrderBy(x => x.CreatedAt);
                }
                else
                {
                    _logger.LogWarning("Unknown order by property: {Property}. Defaulting to Name.", model.OrderBy);
                    query = query.OrderBy(x => x.Name);
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.Name);
            }

            return await _repository.GetPagedAsync(model.PageIndex, model.PageSize, query);
        }
    }
}