using Alma.Core.Data;
using Alma.Core.Types;
using Alma.Organizations.Entities;
using Alma.Organizations.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Alma.Core.Validations;
using Alma.Core.Contexts;

namespace Alma.Organizations.Services
{
    public interface IOrganizationService
    {
        Task<ServiceResult<Organization>> Create(OrganizationCreateModel model);

        Task<Organization> UpdateAsync(OrganizationEditModel model);

        Task<bool> SubdomainExists(string subdomain, string? ignoreId = null);

        Task<bool> Exists(string id);

        ValueTask<Organization?> GetById(string id);

        ValueTask<Organization?> GetBySubdomain(string subdomain);
    }

    public class OrganizationService : IOrganizationService
    {
        private readonly ILogger<OrganizationService> _logger;
        private readonly IUserContext _userContext;
        private readonly IContext _context;
        private readonly IRepository<Organization> _repository;
        private readonly IOrganizationUserService _organizationUserService;
        private readonly IValidator _validator;
        private readonly HybridCache _cache;

        private readonly HybridCacheEntryOptions _cacheEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromHours(1)
        };

        public OrganizationService(ILogger<OrganizationService> logger, IRepository<Organization> repository, IUserContext userContext, IOrganizationUserService organizationUserService, IContext context, IValidator validator, HybridCache cache)
        {
            _logger = logger;
            _repository = repository;
            _userContext = userContext;
            _organizationUserService = organizationUserService;
            _context = context;
            _validator = validator;
            _cache = cache;
        }

        #region Operations

        public async Task<ServiceResult<Organization>> Create(OrganizationCreateModel model)
        {
            _logger.LogDebug("Creating organization {Name} with subdomain: {Subdomain}", model.Name, model.Subdomain);

            #region Normalization

            // Normalize the subdomain
            model.Subdomain = model.Subdomain.ToLowerInvariant();

            #endregion

            #region Validations

            // Validate the model
            var validationResult = await _validator.Validate(model);

            if (!validationResult.IsValid)
                return ServiceResult<Organization>.ValidationError(validationResult);

            // Validate the subdomain
            if (await SubdomainExists(model.Subdomain))
            {
                _logger.LogWarning("Subdomain {Subdomain} already exists.", model.Subdomain);
                return ServiceResult<Organization>.ValidationError("", "Subdomain already exists.");
            }

            #endregion

            var organization = new Organization
            {
                Subdomain = model.Subdomain,
                Name = model.Name
            };

            try
            {
                await _context.BeginTransactionAsync();

                await _repository.InsertAsync(organization);

                var userCreateModel = new OrganizationUserCreateModel
                {
                    OrganizationId = organization.Id,
                    UserId = _userContext.UserId,
                };

                var userCreateResult = await _organizationUserService.CreateAsync(userCreateModel);

                if (!userCreateResult.Succeeded)
                {
                    _logger.LogError("Failed to create organization user for organization {OrganizationId}.\n{@Result}", organization.Id, userCreateResult);
                    await _context.RollbackAsync();
                    return ServiceResult<Organization>.ServerError("Impossible to create organization user.");
                }

                await _context.CommitAsync();

                _logger.LogInformation("Organization {OrganizationId} - {Name} created successfully.", organization.Id, organization.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organization.");

                await _context.RollbackAsync();

                return ServiceResult<Organization>.ServerError("An error occurred while creating the organization.", ex);
            }

            return ServiceResult<Organization>.Success(organization);
        }

        public async Task<Organization> UpdateAsync(OrganizationEditModel model)
        {
            var organization = await GetById(model.Id)
                ?? throw new InvalidOperationException("Organization not found.");

            if (!string.IsNullOrEmpty(model.Subdomain))
            {
                organization.Subdomain = model.Subdomain.ToLowerInvariant();
            }

            if (!string.IsNullOrEmpty(model.Name))
            {
                organization.Name = model.Name;
            }

            await _repository.UpdateAsync(organization);

            // Clear cache for the organization
            await _cache.RemoveAsync($"Organization/{organization.Id}");

            return organization;
        }

        #endregion

        #region Queries

        public ValueTask<Organization?> GetById(string id)
        {
            return _cache.GetOrCreateAsync($"Organization/{id}", async cancel => await _repository.GetOneAsync(x => x.Id == id), _cacheEntryOptions);
        }

        public ValueTask<Organization?> GetBySubdomain(string subdomain)
        {
            var normalizedSubdomain = subdomain.ToLowerInvariant();

            return _cache.GetOrCreateAsync($"Organization/Subdomain/{normalizedSubdomain}", async cancel =>
                await _repository.GetOneAsync(x => x.Subdomain == normalizedSubdomain), _cacheEntryOptions);
        }

        #endregion

        #region Validations

        public async Task<bool> Exists(string id)
        {
            throw new NotImplementedException("Use GetById instead to check existence.");
        }

        public async Task<bool> SubdomainExists(string subdomain, string? ignoredId = null)
        {
            var query = _repository.AsQueryable()
                .Where(x => x.Subdomain == subdomain.ToLowerInvariant());

            if (!string.IsNullOrEmpty(ignoredId))
                query = query.Where(x => x.Id != ignoredId);

            var organization = await _repository.GetPagedAsync(1, 1, query);

            return organization.Count > 0;
        }

        #endregion
    }
}