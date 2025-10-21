using Alma.Workflows.Core.Activities.Models;

namespace Alma.Workflows.Core.Activities.Abstractions
{
    public interface IParameterProvider
    {
        Task<IEnumerable<ParameterOption>> LoadOptionsAsync(string? term = null, string? discriminator = null);
    }
}