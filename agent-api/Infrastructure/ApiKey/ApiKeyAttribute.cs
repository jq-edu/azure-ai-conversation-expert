using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Infrastructure.ApiKey;
public class ApiKeyAttribute : ServiceFilterAttribute
{
    public ApiKeyAttribute()
        : base(typeof(ApiKeyAuthorizationFilter))
    {
    }
}