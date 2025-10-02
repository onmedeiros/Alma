using Alma.Organizations.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Alma.Organizations.Middlewares
{
    internal class OrganizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OrganizationMiddleware> _logger;

        public OrganizationMiddleware(RequestDelegate next, ILogger<OrganizationMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context, IOrganizationContext organizationContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var host = context.Request.Host.Host ?? string.Empty;
            var hostLower = host.ToLowerInvariant();

            // Consider as API only when the subdomain is exactly "api" (e.g., api.localhost, api.example.com)
            // Handles localhost and custom domains. Ports are ignored, since Host.Host excludes port.
            var isApiSubdomain = hostLower == "api" || hostLower == "api.localhost" || hostLower.StartsWith("api.");

            if (!isApiSubdomain)
            {
                // Not an API subdomain: skip middleware logic.
                await _next(context);
                return;
            }

            // Place any logic that must run only for API subdomain requests here.
            // Currently, we just log for traceability and continue.
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("OrganizationMiddleware executing for API subdomain request: {Host}{Path}", context.Request.Host, context.Request.Path);

            // Get the first segment of the path as organization identifier
            var path = context.Request.Path.HasValue ? context.Request.Path.Value! : string.Empty;
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var organizationSubdomain = segments.Length > 0 ? segments[0] : string.Empty;

            var isValid = OrganizationPathHelper.IsValidSubdomain(organizationSubdomain);

            if (!isValid)
            {
                await _next(context);
                return;
            }

            await organizationContext.SetCurrentOrganizationBySubdomain(organizationSubdomain);

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}