namespace AgentApi.Infrastructure;
public interface IApiKeyValidator
{
    bool IsValid(string apiKey);
}