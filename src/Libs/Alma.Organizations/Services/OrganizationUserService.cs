using Alma.Core.Data;
using Alma.Core.Types;
using Alma.Organizations.Entities;
using Alma.Organizations.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Alma.Core.Validations;

namespace Alma.Organizations.Services
{
    public interface IOrganizationUserService
    {
        Task<Core.Types.ServiceResult<OrganizationUser?>> CreateAsync(OrganizationUserCreateModel organizationUser);

        Task<List<OrganizationUser>> GetByOrganizationId(string id);

        Task<PagedList<OrganizationUser>> GetPagedByOrganizationId(string id, int page, int pageSize);

        Task<List<string>> GetOrganizationsAsync(string userId);

        ValueTask SetCurrentOrganizationAsync(string userId, string organizationId);

        ValueTask<string> GetCurrentOrganizationAsync(string userId);
    }

    public class OrganizationUserService : IOrganizationUserService
    {
        private readonly ILogger<OrganizationUserService> _logger;
        private readonly IRepository<OrganizationUser> _repository;
        private readonly IValidator _validator;
        private readonly HybridCache _cache;

        public OrganizationUserService(ILogger<OrganizationUserService> logger, IRepository<OrganizationUser> repository, IValidator validator, HybridCache cache)
        {
            _logger = logger;
            _repository = repository;
            _validator = validator;
            _cache = cache;
        }

        public async Task<Core.Types.ServiceResult<OrganizationUser?>> CreateAsync(OrganizationUserCreateModel model)
        {
            var validationResult = await _validator.Validate(model);

            if (!validationResult.IsValid)
                return Core.Types.ServiceResult<OrganizationUser?>.ValidationError(validationResult);

            try
            {
                var organizationUser = new OrganizationUser
                {
                    OrganizationId = model.OrganizationId,
                    UserId = model.UserId,
                    IsCurrent = model.IsCurrent ?? false
                };

                await _repository.InsertAsync(organizationUser);
                return Core.Types.ServiceResult<OrganizationUser?>.Success(organizationUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organization user.");
                return Core.Types.ServiceResult<OrganizationUser?>.ServerError("An error occurred while creating the organization user.", ex);
            }
        }

        public Task<List<OrganizationUser>> GetByOrganizationId(string id)
        {
            var query = _repository.AsQueryable()
                .Where(x => x.OrganizationId == id);

            return _repository.ToListAsync(query);
        }

        public Task<PagedList<OrganizationUser>> GetPagedByOrganizationId(string id, int page, int pageSize)
        {
            var query = _repository.AsQueryable()
                .Where(x => x.OrganizationId == id);

            return _repository.GetPagedAsync(page, pageSize, query);
        }

        public async Task<List<string>> GetOrganizationsAsync(string userId)
        {
            var query = _repository.AsQueryable()
                .Where(x => x.UserId == userId);

            var users = await _repository.ToListAsync(query);

            return users.ConvertAll(x => x.OrganizationId);
        }

        public async ValueTask SetCurrentOrganizationAsync(string userId, string organizationId)
        {
            var organizationUser =
                await _repository.GetOneAsync(x => x.UserId == userId && x.OrganizationId == organizationId);

            if (organizationUser is null)
            {
                _logger.LogError("Invalid attempt to set current organization for user {UserId} in organization {OrganizationId}. User is not part of the organization.", userId, organizationId);
                throw new InvalidOperationException($"User {userId} is not part of organization {organizationId}.");
            }

            if (organizationUser.IsCurrent)
                return;

            // Reset all other organization users for the same user
            await _repository.BeginUpdate()
                .Set(x => x.IsCurrent, false)
                .Where(x => x.UserId == userId && x.IsCurrent)
                .ExecuteAsync(ignoreMatchedCount: true);

            // Set the current organization user
            organizationUser.IsCurrent = true;

            await _repository.UpdateAsync(organizationUser);

            // Update the cache
            await _cache.RemoveAsync($"CurrentOrganization/{userId}");
            await _cache.SetAsync($"CurrentOrganization/{userId}", organizationId, new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(1)
            });
        }

        public ValueTask<string> GetCurrentOrganizationAsync(string userId)
        {
            return _cache.GetOrCreateAsync($"CurrentOrganization/{userId}", async cancel =>
            {
                var organizationUser = await _repository.GetOneAsync(x => x.UserId == userId && x.IsCurrent);

                if (organizationUser is null)
                    return string.Empty;

                return organizationUser.OrganizationId;
            });
        }
    }
}