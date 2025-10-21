using Alma.Core.Modules;
using Alma.Modules.Workflows.Components.Activities;
using Alma.Modules.Workflows.Components.Registries;
using Alma.Modules.Workflows.Stores;
using Alma.Workflows.Activities.Interaction;
using Alma.Workflows.Core.Categories.Stores;
using Alma.Workflows.Core.CustomActivities.Stores;
using Alma.Workflows.Core.InstanceEndpoints.Stores;
using Alma.Workflows.Core.InstanceExecutions.Stores;
using Alma.Workflows.Core.Instances.Stores;
using Alma.Workflows.Core.InstanceSchedules.Stores;
using Alma.Workflows.Design;
using Alma.Workflows.Hangfire;
using Alma.Workflows.Stores;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using Alma.Modules.Widgets.Extensions;
using Alma.Modules.Workflows.Components.Widgets;
using Alma.Modules.Widgets.Options;

namespace Alma.Modules.Workflows
{
    public class Module : ModuleBase
    {
        public override ModuleDescriptor Descriptor => new ModuleDescriptor
        {
            Name = "Alma.Modules.Workflows",
            Category = ModuleCategory.Management,
            DisplayName = "Fluxos de trabalho",
            Order = 0,
            Icon = Icons.Material.Outlined.AccountTree,
            Menu =
            [
                new ModuleMenuItem{
                    Path = "Workflows/activities",
                    DisplayName = "Atividades"
                },
                new ModuleMenuItem
                {
                    Path = "/Workflows/definitions",
                    DisplayName = "Definições"
                },
                new ModuleMenuItem
                {
                    Path = "/Workflows/instances",
                    DisplayName = "Instâncias"
                },
                new ModuleMenuItem
                {
                    Path = "/Workflows/executions",
                    DisplayName = "Execuções"
                }
            ]
        };

        public override void Configure(IServiceCollection services)
        {
            services.AddHostedService<WorkflowstoreConfigurator>();

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
            services.AddWorkflowsHangfire();

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