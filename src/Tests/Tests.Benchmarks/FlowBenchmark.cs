using Alma.Workflows;
using Alma.Workflows.Activities;
using Alma.Workflows.Activities.Data;
using Alma.Workflows.Builders;
using Alma.Workflows.Core.Activities.Steps;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Models.Activities;
using Alma.Workflows.Parsers;
using Alma.Workflows.Runners;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Benchmarks
{
    [MemoryDiagnoser]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class FlowBenchmark
    {
        private IServiceProvider _serviceProvider = default!;
        private IWorkflowRunnerFactory _runnerFactory = default!;
        private Flow _flow = default!;

        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddAlmaWorkflows(options =>
            {
                // Configure flow options if needed
            });

            _serviceProvider = services.BuildServiceProvider();
            _runnerFactory = _serviceProvider.GetRequiredService<IWorkflowRunnerFactory>();

            var builder = _serviceProvider.GetRequiredService<IFlowBuilder>();

            // Build a sample flow
            builder.AddActivity<StartActivity>("start")
                .WithBeforeExecutionStep<WaitConnectionsStep>()
                .WithBeforeExecutionStep<CheckReadynessStep>()
                .WithBeforeExecutionStep<ApprovalsStep>();

            builder.AddActivity<VariableActivity>("var1")
                .WithParameter(x => x.Type, VariableType.String)
                .WithParameter(x => x.Value, "1")
                .WithBeforeExecutionStep<WaitConnectionsStep>()
                .WithBeforeExecutionStep<CheckReadynessStep>()
                .WithBeforeExecutionStep<ApprovalsStep>();

            builder.AddActivity<VariableActivity>("var2")
                .WithParameter(x => x.Type, VariableType.String)
                .WithParameter(x => x.Value, "3")
                .WithBeforeExecutionStep<WaitConnectionsStep>()
                .WithBeforeExecutionStep<CheckReadynessStep>()
                .WithBeforeExecutionStep<ApprovalsStep>();

            builder.AddActivity<CalculateActivity>("calculate")
                .WithParameter(x => x.Expression, "$var(var1) + $var(var2)")
                .WithBeforeExecutionStep<WaitConnectionsStep>()
                .WithBeforeExecutionStep<CheckReadynessStep>()
                .WithBeforeExecutionStep<ApprovalsStep>();

            builder.AddActivity<OutputVariableActivity>("output")
                .WithParameter(x => x.Name, "result")
                .WithBeforeExecutionStep<WaitConnectionsStep>()
                .WithBeforeExecutionStep<CheckReadynessStep>()
                .WithBeforeExecutionStep<ApprovalsStep>();

            builder.AddActivity<WriteLineActivity>("log")
                .WithParameter(x => x.Message, "The result is: $var(result).")
                .WithBeforeExecutionStep<WaitConnectionsStep>()
                .WithBeforeExecutionStep<CheckReadynessStep>()
                .WithBeforeExecutionStep<ApprovalsStep>();

            builder.AddConnection(options =>
            {
                options.WithId("connection1");
                options.WithSource("start", "Done");
                options.WithTarget("var1", "Input");
            });

            builder.AddConnection(options =>
            {
                options.WithId("connection2");
                options.WithSource("start", "Done");
                options.WithTarget("var2", "Input");
            });

            builder.AddConnection(options =>
            {
                options.WithId("connection3");
                options.WithSource("var1", "Done");
                options.WithTarget("calculate", "Input");
            });

            builder.AddConnection(options =>
            {
                options.WithId("connection4");
                options.WithSource("var2", "Done");
                options.WithTarget("calculate", "Input");
            });

            builder.AddConnection(options =>
            {
                options.WithId("connection5");
                options.WithSource("calculate", "Done");
                options.WithTarget("output", "Input");
            });

            builder.AddConnection(options =>
            {
                options.WithId("connection6");
                options.WithSource("output", "Done");
                options.WithTarget("log", "Input");
            });

            _flow = builder.Build();
        }

        private async Task ExecuteFlowOnceAsync()
        {
            var runner = _runnerFactory.Create(_flow);
            while (await runner.ExecuteNextAsync().ConfigureAwait(false))
            {
                // flow execution
            }
        }

        private async Task ExecuteFlowNTimesAsync(int times)
        {
            for (int i = 0; i < times; i++)
            {
                await ExecuteFlowOnceAsync().ConfigureAwait(false);
            }
        }

        [Benchmark]
        public async Task Run()
        {
            await ExecuteFlowOnceAsync();
        }

        [Benchmark]
        public async Task Run1000()
        {
            await ExecuteFlowNTimesAsync(1000);
        }

        [Benchmark]
        public async Task Run1000Parallel()
        {
            const int total = 1000;
            const int degree = 4;

            var tasks = new List<Task>(degree);
            int baseCount = total / degree;
            int remainder = total % degree;

            for (int i = 0; i < degree; i++)
            {
                int runsForThisTask = baseCount + (i < remainder ? 1 : 0);
                tasks.Add(Task.Run(async () =>
                {
                    for (int j = 0; j < runsForThisTask; j++)
                    {
                        await ExecuteFlowOnceAsync().ConfigureAwait(false);
                    }
                }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}