using Alma.Core.Data;
using Alma.Core.Types;
using Alma.Integrations.Apis.Entities;
using Alma.Integrations.Apis.Models;
using Microsoft.Extensions.Logging;
using Alma.Core.Validations;
using System.Security.Cryptography;

namespace Alma.Integrations.Apis.Services
{
    public interface IApiKeyService
    {
        ValueTask<ServiceResult<ApiKey>> CreateAsync(ApiKeyCreateModel model);

        Task<List<ApiKey>> ListAsync(string apiId, string organizationId);
    }

    public class ApiKeyService : IApiKeyService
    {
        private readonly ILogger<ApiKeyService> _logger;
        private readonly IRepository<ApiKey> _repository;
        private readonly IValidator _validator;

        public ApiKeyService(ILogger<ApiKeyService> logger, IRepository<ApiKey> repository, IValidator validator)
        {
            _logger = logger;
            _repository = repository;
            _validator = validator;
        }

        public async ValueTask<ServiceResult<ApiKey>> CreateAsync(ApiKeyCreateModel model)
        {
            var validation = await _validator.Validate(model);
            if (!validation.IsValid)
                return ServiceResult<ApiKey>.ValidationError(validation);

            var key = new ApiKey
            {
                OrganizationId = model.OrganizationId,
                ApiId = model.ApiId,
                Name = model.Name,
                Key = GenerateSecureKey(),
                IsActive = model.IsActive,
            };

            await _repository.InsertAsync(key);
            return ServiceResult<ApiKey>.Success(key);
        }

        public Task<List<ApiKey>> ListAsync(string apiId, string organizationId)
        {
            var query = _repository.AsQueryable()
                .Where(x => x.ApiId == apiId && x.OrganizationId == organizationId)
                .OrderByDescending(x => x.CreatedAt);

            return _repository.ToListAsync(query);
        }

        private static string GenerateSecureKey(int size = 32)
        {
            Span<byte> bytes = stackalloc byte[size];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes).TrimEnd('=');
        }
    }
}