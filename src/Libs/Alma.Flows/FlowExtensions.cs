using Alma.Flows.Activities;
using Alma.Flows.Activities.Data;
using Alma.Flows.Activities.Flow;
using Alma.Flows.Activities.Integration;
using Alma.Flows.Activities.Interaction;
using Alma.Flows.Activities.ParameterProviders;
using Alma.Flows.ApprovalAndChecks;
using Alma.Flows.Builders;
using Alma.Flows.Core.Activities.Abstractions;
using Alma.Flows.Core.Activities.Factories;
using Alma.Flows.Core.Categories.Services;
using Alma.Flows.Core.CustomActivities.Services;
using Alma.Flows.Core.CustomActivities.Stores;
using Alma.Flows.Core.InstanceEndpoints.Services;
using Alma.Flows.Core.InstanceEndpoints.Stores;
using Alma.Flows.Core.InstanceExecutions.Services;
using Alma.Flows.Core.InstanceExecutions.Stores;
using Alma.Flows.Core.Instances.Services;
using Alma.Flows.Core.Instances.Stores;
using Alma.Flows.Core.InstanceSchedules.Services;
using Alma.Flows.Core.InstanceSchedules.Stores;
using Alma.Flows.Options;
using Alma.Flows.Parsers;
using Alma.Flows.Registries;
using Alma.Flows.Runners;
using Alma.Flows.Scripting;
using Alma.Flows.Scripting.Javascript;
using Alma.Flows.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Flows
{
    public static class FlowExtensions
    {
        public static AlmaFlowBuilder AddAlmaFlows(this IServiceCollection services, Action<FlowOptions> configure)
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
            services.AddScoped<IFlowInstanceManager, FlowInstanceManager>();
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