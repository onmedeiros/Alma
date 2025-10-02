using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpenIddict.Validation.AspNetCore;

namespace Alma.Blazor.Security
{
    public class AlmaAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string[]? _requiredScopes;

        public AlmaAuthorizeAttribute(string[]? scopes = null)
        {
            _requiredScopes = scopes;
            AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (_requiredScopes != null && _requiredScopes.Any())
            {
                var userScopes = user.Claims.Where(c => c.Type == "scope").Select(c => c.Value).ToList();

                if (!_requiredScopes.All(scope => userScopes.Contains(scope)))
                {
                    context.Result = new ForbidResult();
                }
            }
        }
    }
}