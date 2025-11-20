using Alma.Workflows.Databases.Types;

namespace Alma.Workflows.Databases.Providers
{
    public interface IDatabaseProvider
    {
        DatabaseProviderDescriptor Descriptor { get; init; }

        ValueTask<ConnectionResult> ConnectAsync(string connectionString, CancellationToken cancellationToken = default);

        ValueTask<CommandResult<T>> RunCommandAsync<T>(string query, CancellationToken cancellationToken = default);

        ValueTask<CommandResult<string>> RunCommandJsonAsync(string query, CancellationToken cancellationToken = default);
    }
}