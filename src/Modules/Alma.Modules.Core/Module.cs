using Alma.Core.Modules;
using MudBlazor;

namespace Alma.Modules.Core
{
    public class Module : ModuleBase
    {
        public override ModuleDescriptor Descriptor => new ModuleDescriptor
        {
            Name = "Alma.Modules.Core",
            Category = ModuleCategory.Home,
            DisplayName = "Módulo Base",
            Order = 0,
            Icon = Icons.Material.Outlined.Apartment,
            Menu =
            [
                new ModuleMenuItem
                {
                    Path = "/",
                    DisplayName = "Página inicial"
                }
            ]
        };
    }
}