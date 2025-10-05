using Alma.Core.Security;
using MongoDB.Driver.Linq;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Alma.Blazor.Entities;
using Alma.Core.Data;

namespace Alma.Blazor.Services
{
    public interface IIdentityManager
    {
        /// <summary>
        /// Emite um ClaimsPrincipal para a aplicação.
        /// </summary>
        /// <param name="clientId">ClientId da aplicação.</param>
        /// <returns>Retorna um objeto ClaimsPrincipal referente a aplicação com o ClientID especificado.</returns>
        Task<ClaimsPrincipal> IssueForApplication(string clientId);
    }

    public class IdentityManager : IIdentityManager
    {
        private readonly ILogger<IdentityManager> _logger;
        private readonly IRepository<OpenIdApplicationClaim> _applicationClaimRepository;
        private readonly IOpenIddictApplicationManager _applicationManager;

        public IdentityManager(ILogger<IdentityManager> logger, IRepository<OpenIdApplicationClaim> applicationClaimRepository, IOpenIddictApplicationManager applicationManager)
        {
            _logger = logger;
            _applicationClaimRepository = applicationClaimRepository;
            _applicationManager = applicationManager;
            _applicationManager = applicationManager;
        }

        ///<inheritdoc/>
        public async Task<ClaimsPrincipal> IssueForApplication(string clientId)
        {
            // Busca a aplicação pelo clientId.
            // Obs: Tenha certeza de que o ClientId e ClientSecret estão validados.
            var application = await _applicationManager.FindByClientIdAsync(clientId) ??
                                throw new InvalidOperationException("The application cannot be found.");

            // Cria um novo ClaimsIdentity contendo as claims que
            // serão usadas para criar um id_token, um token ou um code.
            var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

            // Usa o client_id como identificador do assunto.
            identity.SetClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application));
            identity.SetClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application));
            identity.SetClaim(AlmaClaims.Organization, (application as AlmaApplication)?.OrganizationId);

            // Adiciona as Claims restantes.
            var claims = await _applicationClaimRepository.AsQueryable()
                .Where(x => x.ClientId == clientId)
                .ToListAsync();

            foreach (var claim in claims)
                identity.SetClaim(claim.Type, claim.Value);

            identity.SetDestinations(static claim => claim.Type switch
            {
                // Permite que a reivindicação "name" seja armazenada nos tokens de acesso e de identidade
                // quando o escopo "profile" foi concedido (chamando principal.SetScopes(...)).
                Claims.Name when claim.Subject!.HasScope(Scopes.Profile)
                    => [Destinations.AccessToken, Destinations.IdentityToken],

                // Caso contrário, armazene a reivindicação apenas nos tokens de acesso.
                _ => [Destinations.AccessToken]
            });

            return new ClaimsPrincipal(identity);
        }
    }
}