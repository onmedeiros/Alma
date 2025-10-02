using Alma.Organizations.Contexts;
using Alma.Organizations.Services;
using Microsoft.Extensions.DependencyInjection;
using Alma.Core;

namespace Alma.Organizations
{
    public static class AlmaOrganizationExtensions
    {
        /// <summary>
        /// Adds Alma organization services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddAlmaOrganizations(this IServiceCollection services)
        {
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IOrganizationUserService, OrganizationUserService>();
            services.AddScoped<IOrganizationContext, OrganizationContext>();

            services.AddSimpleValidator("Alma.Organizations");

            return services;
        }
    }
}