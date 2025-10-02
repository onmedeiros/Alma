using Alma.Blazor.Entities;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Alma.Blazor.HostedServices
{
    public class ApplicationRegister : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public ApplicationRegister(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var organizationId = "0dda0a6e-8356-46ca-bda7-0610bd1f4d89";

            if (await manager.FindByClientIdAsync("medeiros") is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "medeiros",
                    ClientSecret = "medeiros",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.ClientCredentials
                    }
                });

                var application = await manager.FindByClientIdAsync("medeiros");

                ((AlmaApplication)application).OrganizationId = organizationId;

                await manager.UpdateAsync(application);


            }

            //// Add claim
            //var claimManager = scope.ServiceProvider.GetRequiredService<IMongoRepository<OpenIdApplicationClaim>>();
            //var exists = await claimManager.ExistsAsync(c => c.ClientId == "medeiros" && c.Type == "org");

            //if (!exists)
            //{
            //    await claimManager.InsertAsync(new OpenIdApplicationClaim
            //    {
            //        ClientId = "medeiros",
            //        Type = "org",
            //        Value = organizationId
            //    });
            //}
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
