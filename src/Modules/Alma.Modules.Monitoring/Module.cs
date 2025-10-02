using Alma.Core.Modules;
using Alma.Flows.Design.Registries;
using Alma.Flows.Monitoring;
using Alma.Flows.Monitoring.Activities;
using Alma.Modules.Monitoring.Components.ParameterEditors;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Alma.Modules.Monitoring
{
    public class Module : ModuleBase
    {
        public override ModuleDescriptor Descriptor => new ModuleDescriptor
        {
            Name = "Alma.Modules.Monitoring",
            Category = ModuleCategory.Management,
            DisplayName = "Monitoramento",
            Order = 10,
            Icon = Icons.Material.Outlined.MonitorHeart,
            Menu =
            [
                new ModuleMenuItem
                {
                    Path = "/Monitoring/Dashboard",
                    DisplayName = "Dashboard"
                },
                new ModuleMenuItem
                {
                    Path = "/Monitoring/Objects/Schemas",
                    DisplayName = "Estruturas de Objeto",
                    Enabled = true
                }
            ]
        };

        public override void Configure(IServiceCollection services)
        {
            // Add Services
            services.AddAlmaFlowsMonitoring();

            services.ConfigureParameterEditors(options =>
            {
                options.Register<CreateMonitoringObjectActivity, CreateMonitoringObjectParameterEditor>();
                options.Register<AverageMonitoringActivity, AverageMonitoringParameterEditor>();
            });
        }
    }
}