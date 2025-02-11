using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using WCCG.PAS.Referrals.UI.Configs;

namespace WCCG.PAS.Referrals.UI.Repositories;

[ExcludeFromCodeCoverage]
public class CosmosRepository<T> : ICosmosRepository<T>
{
    private readonly Container _container;

    public CosmosRepository(CosmosClient client, CosmosConfig config)
    {
        var database = client.GetDatabase(config.DatabaseName);
        _container = database.GetContainer(config.ContainerName);
    }

    public async Task<bool> UpsertAsync(T item)
    {
        var response = await _container.UpsertItemAsync(item);
        return response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        using var queryResultSetIterator = _container.GetItemLinqQueryable<T>().ToFeedIterator();

        var itemsList = new List<T>();

        while (queryResultSetIterator.HasMoreResults)
        {
            var currentResultSet = await queryResultSetIterator.ReadNextAsync();
            itemsList.AddRange(currentResultSet);
        }

        return itemsList;
    }

    public async Task<T> GetByIdAsync(string id)
    {
        return await _container.ReadItemAsync<T>(id, new PartitionKey(id));
    }
}
