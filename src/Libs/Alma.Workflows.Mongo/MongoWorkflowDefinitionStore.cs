using Alma.Workflows.Definitions;
using Alma.Workflows.Stores;
using Alma.Workflows.Stores.Filters;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SimpleCore.Types;

namespace Alma.Workflows.Mongo
{

    public class MongoFlowDefinitionStore : IFlowDefinitionStore
    {
        private readonly ILogger<MongoFlowDefinitionStore> _logger;
        private readonly IMongoDatabase _database;

        public MongoFlowDefinitionStore(ILogger<MongoFlowDefinitionStore> logger, IMongoClient client)
        {
            _logger = logger;
            _database = client.GetDatabase("Workflows");
        }

        public ValueTask<FlowDefinition> InsertAsync(FlowDefinition definition, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<FlowDefinition> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<FlowDefinition?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }



        public ValueTask<PagedList<FlowDefinition>> ListAsync(int page, int pageSize, FlowDefinitionFilters? filters = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<FlowDefinition> UpdateAsync(FlowDefinition definition, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
