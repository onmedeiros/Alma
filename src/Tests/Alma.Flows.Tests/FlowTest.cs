using Alma.Flows.Activities;
using Alma.Flows.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Flows.Tests
{
    public class FlowTest
    {
        [Test]
        public async Task FlowExecution()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddAlmaFlows(options =>
            {
                options.AddActivity<StartActivity>();
                options.AddActivity<WriteLineActivity>();
            });

            serviceCollection.AddLogging(); // Ensure Microsoft.Extensions.Logging is included

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var builder = serviceProvider.GetRequiredService<IFlowBuilder>();

            // Adiciona as atividades
            var start = builder.AddStart<StartActivity>("1").Build();

            var writeLine1 = builder.AddActivity<WriteLineActivity>("2")
                .WithParameter(x => x.Message, "Hello, World!").Build();

            var writeLine2 = builder.AddActivity<WriteLineActivity>("3")
                .WithParameter("Message", "Goodbye, World!").Build();

            var writeLine3 = builder.AddActivity<WriteLineActivity>("4")
                .WithParameter("Message", "Exit.").Build();

            builder.AddConnection(connection =>
            {
                connection.WithSource(start, x => x.Done);
                connection.WithTarget(writeLine1, "Input");
            });

            builder.AddConnection(connection =>
            {
                connection.WithSource(start, x => x.Done);
                connection.WithTarget(writeLine2, "Input");
            });

            builder.AddConnection(connection =>
            {
                connection.WithSource(writeLine1, x => x.Done)
                    .WithTarget(writeLine3, x => x.Input);
            });

            // Run
            var runner = serviceProvider.GetRequiredService<IFlowRunManager>();
            // await runner.RunAsync(builder.Build(), null);

            //var flow = new Flow();

            //var start = new StartActivity();
            //var writeLine1 = new WriteLineActivity();
            //var writeLine2 = new WriteLineActivity();
            //var writeLine3 = new WriteLineActivity();
            //var writeLine4 = new WriteLineActivity();

            //writeLine1.Message = new Parameter<string>("Hello, World!");
            //writeLine2.Message = new Parameter<string>("Goodbye, World!");
            //writeLine3.Message = new Parameter<string>("Activity 3!");
            //writeLine4.Message = new Parameter<string>("Activity 4!");


            //var connection1 = new Connection
            //{
            //    Source = new Endpoint(start, "Done"),
            //    Target = new Endpoint(writeLine1, "In")
            //};

            //var connection2 = new Connection
            //{
            //    Source = new Endpoint(start, "Done"),
            //    Target = new Endpoint(writeLine2, "In")
            //};

            //var connection3 = new Connection
            //{
            //    Source = new Endpoint(writeLine2, "Done"),
            //    Target = new Endpoint(writeLine3, "In")
            //};

            //var connection4 = new Connection
            //{
            //    Source = new Endpoint(writeLine3, "Done"),
            //    Target = new Endpoint(writeLine4, "In")
            //};

            //var connection5 = new Connection
            //{
            //    Source = new Endpoint(writeLine2, "Done"),
            //    Target = new Endpoint(writeLine4, "In")
            //};

            //flow.Activities.Add(start);
            //flow.Activities.Add(writeLine1);
            //flow.Activities.Add(writeLine2);
            //flow.Activities.Add(writeLine3);
            //flow.Activities.Add(writeLine4);

            //flow.AddConnection(connection1);
            //flow.AddConnection(connection2);
            //flow.AddConnection(connection3);
            //flow.AddConnection(connection4);
            //flow.AddConnection(connection5);

            //flow.Start = start;


        }
    }
}