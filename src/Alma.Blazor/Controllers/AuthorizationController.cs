using Alma.Blazor.Security;
using Alma.Blazor.Services;
using Alma.Core.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;

namespace Alma.Blazor.Controllers
{
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly IIdentityManager _identityManager;

        public AuthorizationController(ILogger<AuthorizationController> logger, IIdentityManager identityManager)
        {
            _logger = logger;
            _identityManager = identityManager;
        }

        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Token()
        {
            var request = HttpContext.GetOpenIddictServerRequest();

            if (request == null)
                throw new Exception("The OpenIddict request cannot be retrieved.");

            if (request.IsClientCredentialsGrantType())
            {
                var identity = await _identityManager.IssueForApplication(request.ClientId!);
                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new NotImplementedException("The specified grant is not implemented.");
        }

        [HttpGet("~/connect/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            await HttpContext.SignOutAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            return SignOut(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = "/"
                });
        }

        [HttpGet("~/connect/secure")]
        [AlmaAuthorize]
        public IActionResult Secure()
        {
            return Content("This is a secure endpoint.");
        }
    }
}