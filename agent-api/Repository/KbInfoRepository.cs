using AgentApi.Infrastructure;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace AgentApi.Repository;

public class KbInfoRepository : CosmosDbRepository
{
    public KbInfoRepository(CosmosConfiguration options, DefaultAzureCredential credential) : base(options, credential)
    {
    }
}