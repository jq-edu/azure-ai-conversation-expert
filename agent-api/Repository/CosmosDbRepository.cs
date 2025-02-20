using AgentApi.Infrastructure;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

namespace AgentApi.Repository;

public abstract class CosmosDbRepository : ICosmosDbRepository
{
    protected readonly Container _container;

    public CosmosDbRepository(CosmosConfiguration options, DefaultAzureCredential credential)
    {
        var uri = options.Uri;
        if (string.IsNullOrEmpty(uri))
        {
            throw new ArgumentNullException(nameof(uri), "The CosmosDb:Uri configuration value is missing or empty.");
        }

        var databaseId = options.DatabaseId;
        if (string.IsNullOrEmpty(databaseId))
        {
            throw new ArgumentNullException(nameof(databaseId), "The CosmosDb:DatabaseId configuration value is missing or empty.");
        }

        var containerId = options.ContainerId;
        if (string.IsNullOrEmpty(containerId))
        {
            throw new ArgumentNullException(nameof(containerId), "The CosmosDb:ContainerId configuration value is missing or empty.");
        }

        CosmosSerializationOptions cosmosOptions = new()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        };

        CosmosClient client = new CosmosClientBuilder(uri, credential)
                                    .WithSerializerOptions(cosmosOptions)
                                    .Build();
        Database db = client.GetDatabase(databaseId);
        _container = db.GetContainer(containerId);
    }

    public async Task<T> InsertAsync<T>(T item) where T : class
    {
        return await _container.UpsertItemAsync(item);
    }

    public async Task<IEnumerable<T>> GetItems<T>(string query, IDictionary<string, object> parameters) where T : class
    {
        QueryDefinition queryDefinition = new QueryDefinition(query);

        foreach (var p in parameters)
        {
            queryDefinition.WithParameter(p.Key, p.Value);
        }

        FeedIterator<T> response = _container.GetItemQueryIterator<T>(queryDefinition);

        List<T> entities = new List<T>();
        while (response.HasMoreResults)
        {
            FeedResponse<T> results = await response.ReadNextAsync();
            entities.AddRange(results);
        }
        return entities;
    }
}
