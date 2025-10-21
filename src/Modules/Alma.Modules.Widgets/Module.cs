using Alma.Core.Modules;
using Alma.Modules.Dashboards.Interop;
using Alma.Modules.Widgets.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Alma.Modules.Dashboards
{
    public class Module : ModuleBase
    {
        public override ModuleDescriptor Descriptor => new ModuleDescriptor
        {
            Name = "Alma.Modules.Dashboards",
            Category = ModuleCategory.Home,
            DisplayName = "Dashboard",
            Order = 10,
            Icon = Icons.Material.Outlined.Dashboard,
            Menu =
            [
                new ModuleMenuItem
                {
                    Path = "/Dashboard",
                    DisplayName = "Dashboard"
                }
            ]
        };

        public override void Configure(IServiceCollection services)
        {
            services.AddScoped<IGridstackInterop, GridStackInterop>();
            services.AddScoped<IWidgetRegistry, WidgetRegistry>();
            services.AddScoped<IWidgetService, WidgetService>();
        }
    }
}