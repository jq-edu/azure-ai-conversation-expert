
namespace AgentApi.Infrastructure
{
    public class CosmosConfiguration
    {
        public required string Uri { get; set; }
        public required string DatabaseId { get; set; }
        public required string ContainerId { get; set; }
    }
}