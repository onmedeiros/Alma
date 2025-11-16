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
using Alma.Workflows.Runners.Connections;
using Alma.Workflows.Runners.Coordination;
using Alma.Workflows.Runners.ExecutionModes;
using Alma.Workflows.Runners.Queue;
using Alma.Workflows.Runners.Strategies;
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
            
            // Fase 1: Coordenação de Execução e Modos
            services.AddScoped<IExecutionCoordinator, Runners.Coordination.ExecutionCoordinator>();
            services.AddScoped<IActivityExecutor, Runners.Coordination.ActivityExecutor>();
            services.AddScoped<IExecutionModeStrategyFactory, Runners.ExecutionModes.ExecutionModeStrategyFactory>();
            services.AddScoped<Runners.ExecutionModes.ManualExecutionModeStrategy>();
            services.AddScoped<Runners.ExecutionModes.StepByStepExecutionModeStrategy>();
            services.AddScoped<Runners.ExecutionModes.AutomaticExecutionModeStrategy>();
            
            // Fase 1: Gerenciamento de Conexões
            // ConnectionManager mantém cache interno; evitar compartilhamento entre execuções concorrentes
            services.AddTransient<IConnectionManager, Runners.Connections.ConnectionManager>();
            
            // Fase 2: Property Accessors (High-Performance Reflection Alternative)
            services.AddSingleton<Core.Properties.PropertyAccessorFactory>();
            services.AddSingleton<Core.Properties.ParameterAccessor>();
            services.AddSingleton<Core.Properties.DataAccessor>();
            services.AddSingleton<Core.Properties.PortAccessor>();
            
            // Fase 2: Activity Visitors (Operations without modifying Activity classes)
            services.AddTransient<Core.Activities.Visitors.ActivityDescriptionVisitor>();
            services.AddTransient<Core.Activities.Visitors.ActivityValidationVisitor>();
            services.AddTransient<Core.Activities.Visitors.ActivityCloningVisitor>();
            
            // Fase 3: State Components (Separated State Management)
            services.AddTransient<Core.States.Components.IQueueState, Core.States.Components.QueueState>();
            services.AddTransient<Core.States.Components.IVariableState, Core.States.Components.VariableState>();
            services.AddTransient<Core.States.Components.IParameterState, Core.States.Components.ParameterState>();
            services.AddTransient<Core.States.Components.IActivityDataState, Core.States.Components.ActivityDataState>();
            services.AddTransient<Core.States.Components.IApprovalState, Core.States.Components.ApprovalState>();
            services.AddTransient<Core.States.Components.ILogState, Core.States.Components.LogState>();
            
            // Fase 3: State Machine Pattern
            services.AddTransient<Core.States.Machine.IFlowStateMachine, Core.States.Machine.FlowStateMachine>();
            services.AddSingleton<Core.States.Machine.StateTransitionValidator>();
            
            // Fase 3: State Observers (Notification of state changes)
            services.AddTransient<Core.States.Observers.LoggingStateObserver>();
            services.AddSingleton<Core.States.Observers.MetricsStateObserver>();
            
            // Queue Management - Separated Responsibilities (FASE 1.1)
            services.AddTransient<IQueueEnqueuer, QueueEnqueuer>();
            services.AddTransient<IQueueNavigator, QueueNavigator>();
            services.AddTransient<IQueueStateManager, QueueStateManager>();
            services.AddTransient<IQueueStatusResolver, QueueStatusResolver>();
            services.AddTransient<IQueueManager, QueueManagerFacade>();
            
            services.AddTransient<IParameterSetter, ParameterSetter>();
            services.AddTransient<IDataSetter, DataSetter>();

            // Activity execution strategies
            services.AddScoped<StandardActivityExecutionStrategy>();
            services.AddScoped<IActivityExecutionStrategy, LoopActivityExecutionStrategy>();
            services.AddScoped<IActivityExecutionStrategy, UserInteractionActivityExecutionStrategy>();
            services.AddScoped<IActivityExecutionStrategy>(sp => sp.GetRequiredService<StandardActivityExecutionStrategy>());
            services.AddScoped<IActivityExecutionStrategyResolver, ActivityExecutionStrategyResolver>();

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