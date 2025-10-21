using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.Instances.Services;

namespace Alma.Workflows.Activities.ParameterProviders
{
    public class InstanceParameterProvider : IParameterProvider
    {
        private readonly IInstanceManager _instanceManager;

        public InstanceParameterProvider(IInstanceManager instanceManager)
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