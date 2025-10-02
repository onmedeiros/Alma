using Alma.Core.Security;
using Alma.Organizations.Entities;
using Alma.Organizations.Services;
using Microsoft.Extensions.Logging;
using Alma.Core.Contexts;

namespace Alma.Organizations.Contexts
{
    public interface IOrganizationContext
    {
        ValueTask<Organization?> GetCurrentOrganization();

        ValueTask<string> GetCurrentOrganizationId();

        ValueTask<string?> TryGetCurrentOrganizationId();

        ValueTask SetCurrentOrganization(string userId, string organizationId);

        ValueTask SetCurrentOrganization(string organizationId);

        ValueTask SetCurrentOrganizationBySubdomain(string subdomain);
    }

    public class OrganizationContext : IOrganizationContext
    {
        private readonly ILogger<OrganizationContext> _logger;
        private readonly IUserContext _userContext;
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationUserService _organizationUserService;

        private string _currentOrganizationId = string.Empty;

        public OrganizationContext(ILogger<OrganizationContext> logger, IUserContext userContext, IOrganizationService organizationService, IOrganizationUserService organizationUserService)
        {
            _logger = logger;
            _userContext = userContext;
            _organizationService = organizationService;
            _organizationUserService = organizationUserService;
        }

        public async ValueTask<Organization?> GetCurrentOrganization()
        {
            string? organizationId = null;

            if (_userContext.IsAuthenticated)
                organizationId = GetCurrentOrganizationByClaimsPrincipal();
            else
                organizationId = _currentOrganizationId;

            if (string.IsNullOrEmpty(organizationId))
                organizationId = await _organizationUserService.GetCurrentOrganizationAsync(_userContext.UserId);

            if (string.IsNullOrEmpty(organizationId))
                return null;

            return await _organizationService.GetById(organizationId);
        }

        public async ValueTask<string> GetCurrentOrganizationId()
        {
            string? organizationId = null;

            if (_userContext.IsAuthenticated)
            {
                organizationId = GetCurrentOrganizationByClaimsPrincipal();

                if (string.IsNullOrEmpty(organizationId))
                    organizationId = await _organizationUserService.GetCurrentOrganizationAsync(_userContext.UserId);
            }
            else
                organizationId = _currentOrganizationId;

            if (string.IsNullOrEmpty(organizationId))
                throw new InvalidOperationException("Current organization is not set.");

            return organizationId;
        }

        public async ValueTask<string?> TryGetCurrentOrganizationId()
        {
            string? organizationId = null;

            if (_userContext.IsAuthenticated)
            {
                organizationId = GetCurrentOrganizationByClaimsPrincipal();

                if (string.IsNullOrEmpty(organizationId))
                    organizationId = await _organizationUserService.GetCurrentOrganizationAsync(_userContext.UserId);
            }
            else
                organizationId = _currentOrganizationId;

            return organizationId;
        }

        public async ValueTask SetCurrentOrganization(string userId, string organizationId)
        {
            await _organizationUserService.SetCurrentOrganizationAsync(userId, organizationId);
        }

        public ValueTask SetCurrentOrganization(string organizationId)
        {
            _currentOrganizationId = organizationId;
            return ValueTask.CompletedTask;
        }

        public async ValueTask SetCurrentOrganizationBySubdomain(string subdomain)
        {
            var organization = await _organizationService.GetBySubdomain(subdomain);

            if (organization is null)
                throw new InvalidOperationException($"Organization with subdomain '{subdomain}' not found.");

            await SetCurrentOrganization(organization.Id);
        }

        private string? GetCurrentOrganizationByClaimsPrincipal()
        {
            var claimsPrincipal = _userContext.User;

            if (claimsPrincipal?.Claims.Any(x => x.Type == AlmaClaims.Organization) != true)
                return null;

            return claimsPrincipal.FindFirst(AlmaClaims.Organization)?.Value ?? string.Empty;
        }
    }
}