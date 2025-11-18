using Alma.Workflows.Databases.Types;

namespace Alma.Workflows.Databases.Providers
{
    public interface IDatabaseProvider
    {
        DatabaseProviderDescriptor Descriptor { get; init; }

        ValueTask<ConnectionResult> ConnectAsync(string connectionString, CancellationToken cancellationToken = default);

        ValueTask<QueryResult<T>> QueryAsync<T>(string query, CancellationToken cancellationToken = default);

        ValueTask<QueryResult<string>> QueryJsonAsync(string query, CancellationToken cancellationToken = default);
    }
}