using Alma.Core.Modules;
using MudBlazor;

namespace Alma.Modules.Organizations
{
    public class Module : ModuleBase
    {
        public override ModuleDescriptor Descriptor => new ModuleDescriptor
        {
            Name = "Alma.Modules.Organization",
            Category = ModuleCategory.Settings,
            DisplayName = "Módulo Base",
            Order = 100,
            Icon = Icons.Material.Outlined.Settings,
            Menu =
            [
                new ModuleMenuItem
                {
                    Path = "/organization/settings",
                    DisplayName = "Organização"
                }
            ]
        };
    }
}