using Alma.Core.Modules;
using Alma.Modules.Flows.Components.Activities;
using Alma.Modules.Flows.Components.Registries;
using Alma.Modules.Flows.Stores;
using Alma.Flows.Activities.Interaction;
using Alma.Flows.Core.Categories.Stores;
using Alma.Flows.Core.CustomActivities.Stores;
using Alma.Flows.Core.InstanceEndpoints.Stores;
using Alma.Flows.Core.InstanceExecutions.Stores;
using Alma.Flows.Core.Instances.Stores;
using Alma.Flows.Core.InstanceSchedules.Stores;
using Alma.Flows.Design;
using Alma.Flows.Hangfire;
using Alma.Flows.Stores;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using Alma.Modules.Widgets.Extensions;
using Alma.Modules.Flows.Components.Widgets;
using Alma.Modules.Widgets.Options;

namespace Alma.Modules.Flows
{
    public class Module : ModuleBase
    {
        public override ModuleDescriptor Descriptor => new ModuleDescriptor
        {
            Name = "Alma.Modules.Flows",
            Category = ModuleCategory.Management,
            DisplayName = "Fluxos de trabalho",
            Order = 0,
            Icon = Icons.Material.Outlined.AccountTree,
            Menu =
            [
                new ModuleMenuItem{
                    Path = "flows/activities",
                    DisplayName = "Atividades"
                },
                new ModuleMenuItem
                {
                    Path = "/flows/definitions",
                    DisplayName = "Definições"
                },
                new ModuleMenuItem
                {
                    Path = "/flows/instances",
                    DisplayName = "Instâncias"
                },
                new ModuleMenuItem
                {
                    Path = "/flows/executions",
                    DisplayName = "Execuções"
                }
            ]
        };

        public override void Configure(IServiceCollection services)
        {
            services.AddHostedService<FlowStoreConfigurator>();

            // Stores
            services.AddScoped<IFlowDefinitionStore, MongoFlowDefinitionStore>();
            services.AddScoped<IFlowDefinitionVersionStore, MongoFlowDefinitionVersionStore>();
            services.AddScoped<IFlowInstanceStore, MongoFlowInstanceStore>();
            services.AddScoped<IInstanceScheduleStore, MongoInstanceScheduleStore>();
            services.AddScoped<IInstanceEndpointStore, MongoInstanceEndpointStore>();
            services.AddScoped<IInstanceExecutionStore, MongoInstanceExecutionStore>();
            services.AddScoped<ICategoryStore, MongoCategoryStore>();
            services.AddScoped<ICustomActivityTemplateStore, MongoCustomActivityTemplateStore>();

            // Design
            services.AddFlowDesign();

            // Hangfire
            services.AddFlowsHangfire();

            // ActivityComponents
            services.AddActivityComponentRegistry(options =>
            {
                options.Register<FormActivity, FormActivityComponent>();
                options.Register<InstructionActivity, InstructionActivityComponent>();
            });

            // Widgets
            services.ConfigureWidgets(options =>
            {
                options.Register<ActiveInstancesWidget>(new WidgetOptions
                {
                    Name = "Instâncias ativas",
                    Container = "Dashboard",
                    Width = 2,
                    Height = 1,
                    MaxWidth = 4,
                    MaxHeight = 2,
                    MinWidth = 2,
                    MinHeight = 1
                });
            });
        }
    }
}