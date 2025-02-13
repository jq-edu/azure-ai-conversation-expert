namespace AgentApi.Infrastructure.ApiKey;
public interface IApiKeyValidator
{
    bool IsValid(string apiKey);
}