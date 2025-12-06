using Alma.Core.Modules;
using Alma.Modules.Auctions.AwardedLots.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Alma.Modules.Auctions
{
    public class Module : ModuleBase
    {
        public override ModuleDescriptor Descriptor => new ModuleDescriptor
        {
            Name = "Alma.Modules.Auctions",
            Category = ModuleCategory.Management,
            DisplayName = "Leilões",
            Order = 10,
            Icon = Icons.Material.Outlined.Gavel,
            Menu =
            [
                new ModuleMenuItem
                {
                    Path = "/Auctions/AwardedLots",
                    DisplayName = "Lotes arrematados"
                },
                new ModuleMenuItem{
                    Path = "/Auctions/Products",
                    DisplayName = "Produtos"
                },
                new ModuleMenuItem {
                    Path = "/Auctions/Sales",
                    DisplayName = "Vendas"
                }
            ]
        };

        public override void Configure(IServiceCollection services)
        {
            // Register FluentValidation validators
            services.AddValidatorsFromAssemblyContaining<Module>();

            // Register AwardedLot services
            services.AddScoped<IAwardedLotService, AwardedLotService>();
        }
    }
}