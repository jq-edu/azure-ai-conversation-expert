using AgentApi.Infrastructure;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace AgentApi.Repository;

public class ConversationRepository : CosmosDbRepository
{
    public ConversationRepository(CosmosConfiguration options, DefaultAzureCredential credential) : base(options, credential)
    {
    }
}
