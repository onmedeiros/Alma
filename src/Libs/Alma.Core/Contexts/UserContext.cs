using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Alma.Core.Contexts
{
    public interface IUserContext
    {
        bool IsAuthenticated { get; }
        ClaimsPrincipal User { get; }
        string UserId { get; }
    }

    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public ClaimsPrincipal User => GetUser();
        public string UserId => GetUserId();

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserId()
        {
            var user = GetUser();

            if (user.Identity == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User is not authenticated or is invalid.");
        }

        private ClaimsPrincipal GetUser()
        {
            var user = _httpContextAccessor.HttpContext?.User
                ?? throw new UnauthorizedAccessException("User is not authenticated.");

            return user;
        }
    }
}