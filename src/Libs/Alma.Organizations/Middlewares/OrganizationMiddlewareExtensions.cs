using Microsoft.AspNetCore.Builder;

namespace Alma.Organizations.Middlewares
{
    public static class OrganizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseOrganizationMiddleware(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            return app.UseMiddleware<OrganizationMiddleware>();
        }
    }
}