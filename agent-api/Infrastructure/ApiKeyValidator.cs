namespace AgentApi.Infrastructure;
public class ApiKeyValidator : IApiKeyValidator
{
    private readonly string _expectedApiKey;

    public ApiKeyValidator(string expectedApiKey)
    {
        _expectedApiKey = expectedApiKey;
    }

    public bool IsValid(string apiKey)
    {
        return apiKey == _expectedApiKey;
    }
}
