using Alma.Flows.Core.Activities.Abstractions;
using Alma.Flows.Core.Activities.Models;
using Alma.Flows.Core.Instances.Services;

namespace Alma.Flows.Activities.ParameterProviders
{
    public class InstanceParameterProvider : IParameterProvider
    {
        private readonly IFlowInstanceManager _instanceManager;

        public InstanceParameterProvider(IFlowInstanceManager instanceManager)
        {
            _instanceManager = instanceManager;
        }

        public async Task<IEnumerable<ParameterOption>> LoadOptionsAsync(string? term = null, string? discriminator = null)
        {
            var instances = await _instanceManager.List(1, int.MaxValue, new Core.Instances.Stores.FlowInstanceFilters
            {
                Discriminator = discriminator,
                Name = term
            });

            return instances.Items.OrderBy(x => x.Name).Select(i => new ParameterOption
            {
                DisplayName = i.Name,
                Value = i.Id
            });
        }
    }
}