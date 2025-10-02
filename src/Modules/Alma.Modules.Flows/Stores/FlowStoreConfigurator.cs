using Alma.Flows.Core.InstanceExecutions.Entities;
using Alma.Flows.Core.Instances.Entities;
using Alma.Flows.Definitions;
using Alma.Flows.Models.Activities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Alma.Modules.Flows.Stores
{
    public class FlowStoreConfigurator : BackgroundService
    {
        private readonly ILogger<FlowStoreConfigurator> _logger;

        public FlowStoreConfigurator(ILogger<FlowStoreConfigurator> logger)
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