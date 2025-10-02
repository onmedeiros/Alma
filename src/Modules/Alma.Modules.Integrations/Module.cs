using Alma.Core.Modules;
using Alma.Integrations;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Alma.Modules.Integrations
{
    public class Module : ModuleBase
    {
        public override ModuleDescriptor Descriptor => new ModuleDescriptor
        {
            Name = "Alma.Modules.Integrations",
            Category = ModuleCategory.Management,
            DisplayName = "Integrações",
            Order = 10,
            Icon = Icons.Material.Outlined.Api,
            Menu =
            [
                new ModuleMenuItem
                {
                    Path = "/Integrations/Apis",
                    DisplayName = "API"
                },
                new ModuleMenuItem
                {
                    Path = "/Integrations/Databases",
                    DisplayName = "Banco de dados",
                    Enabled = false
                }
            ]
        };

        public override void Configure(IServiceCollection services)
        {
            // Add Services
            services.AddAlmaIntegrations();
        }
    }
}