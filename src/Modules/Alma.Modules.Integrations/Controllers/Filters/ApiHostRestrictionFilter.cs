using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;

namespace Alma.Modules.Integrations.Controllers.Filters
{
    // In Development, only allow requests with host "api.localhost". In other environments, allow all.
    public sealed class ApiHostRestrictionFilter : IAsyncResourceFilter
    {
        private readonly IHostEnvironment _env;

        public ApiHostRestrictionFilter(IHostEnvironment env)
        {
            _env = env;
        }

        public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var host = context.HttpContext.Request.Host.Host;

            if (!host.StartsWith("api."))
            {
                context.Result = new NotFoundResult();
                return Task.CompletedTask;
            }

            return next();
        }
    }
}