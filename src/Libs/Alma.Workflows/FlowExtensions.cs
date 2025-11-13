using Alma.Workflows.Activities;
using Alma.Workflows.Activities.Data;
using Alma.Workflows.Activities.Flow;
using Alma.Workflows.Activities.Integration;
using Alma.Workflows.Activities.Interaction;
using Alma.Workflows.Activities.ParameterProviders;
using Alma.Workflows.ApprovalAndChecks;
using Alma.Workflows.Builders;
using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.Activities.Factories;
using Alma.Workflows.Core.Categories.Services;
using Alma.Workflows.Core.CustomActivities.Services;
using Alma.Workflows.Core.CustomActivities.Stores;
using Alma.Workflows.Core.InstanceEndpoints.Services;
using Alma.Workflows.Core.InstanceEndpoints.Stores;
using Alma.Workflows.Core.InstanceExecutions.Services;
using Alma.Workflows.Core.InstanceExecutions.Stores;
using Alma.Workflows.Core.Instances.Services;
using Alma.Workflows.Core.Instances.Stores;
using Alma.Workflows.Core.InstanceSchedules.Services;
using Alma.Workflows.Core.InstanceSchedules.Stores;
using Alma.Workflows.Options;
using Alma.Workflows.Parsers;
using Alma.Workflows.Registries;
using Alma.Workflows.Runners;
using Alma.Workflows.Scripting;
using Alma.Workflows.Scripting.Javascript;
using Alma.Workflows.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows
{
    public static class FlowExtensions
    {
        public static AlmaFlowBuilder AddAlmaWorkflows(this IServiceCollection services, Action<FlowOptions> configure)
        {
            services.Configure(configure);

            // Add some activities
            services.Configure<FlowOptions>(options =>
            {
                options.AddActivity<StartActivity>();
                options.AddActivity<WriteLineActivity>();

                // Data
                options.AddActivity<VariableActivity>();
                options.AddActivity<OutputVariableActivity>();
                options.AddActivity<CalculateActivity>();

                // Flow
                options.AddActivity<ConditionActivity>();
                options.AddActivity<ScheduleInstanceActivity>();
                options.AddActivity<LoopActivity>();

                // Integration
                options.AddActivity<HttpRequestActivity>();
                options.AddActivity<HttpResponseActivity>();
                options.AddActivity<SshActivity>();

                // Interation
                options.AddActivity<UserInteractionActivity>();
                options.AddActivity<FormActivity>();
                options.AddActivity<InstructionActivity>();

                // Approvals and Checks
                options.AddApprovalAndCheck<BasicApproval>();
            });

            // Add services
            services.AddTransient<IFlowBuilder, FlowBuilder>();
            services.AddTransient<IActivityBuilder, ActivityBuilder>();
            services.AddTransient(typeof(IActivityBuilder<>), typeof(ActivityBuilder<>));

            services.AddSingleton<IActivityRegistry, ActivityRegistry>(); // TODO: Avaliar possibilidade de deixar como Singleton.
            services.AddSingleton<IApprovalAndCheckRegistry, ApprovalAndCheckRegistry>();
            services.AddScoped<ICustomActivityRegistry, CustomActivityRegistry>();
            services.AddScoped<IFlowManager, FlowManager>();
            services.AddScoped<IInstanceManager, InstanceManager>();
            services.AddScoped<IFlowRunManager, FlowRunManager>();
            services.AddScoped<IFlowDefinitionParser, FlowDefinitionParser>();
            services.AddScoped<IInstanceScheduleManager, InstanceScheduleManager>();
            services.AddScoped<IInstanceScheduleJobManager, InstanceScheduleJobManager>();
            services.AddScoped<IInstanceScheduleRunner, InstanceScheduleRunner>();
            services.AddScoped<IInstanceEndpointManager, InstanceEndpointManager>();
            services.AddScoped<IInstanceExecutionManager, InstanceExecutionManager>();
            services.AddScoped<IInstanceExecutionRunner, InstanceExecutionRunner>();
            services.AddScoped<ICategoryManager, CategoryManager>();
            services.AddScoped<ICustomActivityManager, CustomActivityManager>();

            // Execution services
            services.AddScoped<IActivityRunnerFactory, ActivityRunnerFactory>();
            services.AddScoped<IFlowRunnerFactory, FlowRunnerFactory>();
            services.AddScoped<IActivityStepFactory, ActivityStepFactory>();
            services.AddScoped<IApprovalAndCheckResolverFactory, ApprovalAndCheckResolverFactory>();
            services.AddTransient<IQueueManager, QueueManager>();
            services.AddTransient<IParameterSetter, ParameterSetter>();
            services.AddTransient<IDataSetter, DataSetter>();

            // Add stores
            services.AddScoped<IFlowDefinitionStore, FlowDefinitionStore>();
            services.AddScoped<IFlowDefinitionVersionStore, FlowDefinitionVersionStore>();
            services.AddScoped<IFlowInstanceStore, FlowInstanceStore>();
            services.AddScoped<IInstanceScheduleStore, InstanceScheduleStore>();
            services.AddScoped<IInstanceEndpointStore, InstanceEndpointStore>();
            services.AddScoped<IInstanceExecutionStore, InstanceExecutionStore>();
            services.AddScoped<ICustomActivityTemplateStore, CustomActivityStore>();

            // Add parameter options loaders
            services.AddKeyedScoped<IParameterProvider, InstanceParameterProvider>(typeof(InstanceParameterProvider));

            // Add scripting engines
            services.AddKeyedScoped<IScriptEngine, JavaScriptEngine>(ScriptLanguage.JavaScript);

            return new AlmaFlowBuilder(services);
        }
    }
}