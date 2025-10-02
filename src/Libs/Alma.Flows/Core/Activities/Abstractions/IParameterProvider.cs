using Alma.Flows.Core.Activities.Models;

namespace Alma.Flows.Core.Activities.Abstractions
{
    public interface IParameterProvider
    {
        Task<IEnumerable<ParameterOption>> LoadOptionsAsync(string? term = null, string? discriminator = null);
    }
}