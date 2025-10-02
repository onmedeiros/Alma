using Alma.Core.Modules;
using Alma.Flows.Alerts;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Alma.Modules.Alerts
{
    public class Module : ModuleBase
    {
        public override ModuleDescriptor Descriptor => new ModuleDescriptor
        {
            Name = "Alma.Modules.Alerts",
            Category = ModuleCategory.Management,
            DisplayName = "Alertas",
            Order = 10,
            Icon = Icons.Material.Outlined.ReportProblem,
            Menu =
            [
                new ModuleMenuItem
                {
                    Path = "/Alerts",
                    DisplayName = "Alertas"
                }
            ]
        };

        public override void Configure(IServiceCollection services)
        {
            // Add Services
            services.AddFlowAlerts();
        }
    }
}