using Alma.Workflows.Core.InstanceExecutions.Entities;
using Alma.Workflows.Core.Instances.Entities;
using Alma.Workflows.Definitions;
using Alma.Workflows.Models.Activities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Alma.Modules.Workflows.Stores
{
    public class WorkflowstoreConfigurator : BackgroundService
    {
        private readonly ILogger<WorkflowstoreConfigurator> _logger;

        public WorkflowstoreConfigurator(ILogger<WorkflowstoreConfigurator> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Log
            _logger.LogInformation("Configuring Flow Stores.");

            // Map
            MapFlowDefinition();
            MapFlowInstance();
            MapInstanceExecution();
            MapOthers();

            // Create indexes

            // Log
            _logger.LogInformation("Flow Stores configured.");

            return Task.CompletedTask;
        }

        private void MapFlowDefinition()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(ActivityDefinition)))
            {
                BsonClassMap.RegisterClassMap<ActivityDefinition>(map =>
                {
                    map.AutoMap();
                    map.MapIdProperty(x => x.Id);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(FlowDefinition)))
            {
                BsonClassMap.RegisterClassMap<FlowDefinition>(map =>
                {
                    map.AutoMap(); // Isso já inclui o mapeamento da classe base
                    map.SetDiscriminatorIsRequired(true);
                });
            }
        }

        private void MapFlowInstance()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(FlowInstance)))
            {
                BsonClassMap.RegisterClassMap<FlowInstance>(map =>
                {
                    map.AutoMap();
                    map.MapIdProperty(x => x.Id);
                });
            }
        }

        private void MapInstanceExecution()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(InstanceExecution)))
            {
                BsonClassMap.RegisterClassMap<InstanceExecution>(map =>
                {
                    map.AutoMap();
                    map.MapIdProperty(x => x.Id);
                });
            }
        }

        private void MapOthers()
        {
            // Map other classes here
            if (!BsonClassMap.IsClassMapRegistered(typeof(HttpRequestActivityResponse)))
            {
                BsonClassMap.RegisterClassMap<HttpRequestActivityResponse>(map =>
                {
                    map.AutoMap();
                });
            }

            var serializer = new ObjectSerializer(allowedTypes: type => type == typeof(HttpRequestActivityResponse));
            BsonSerializer.RegisterSerializer(typeof(HttpRequestActivityResponse), serializer);
        }
    }
}