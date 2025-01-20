using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Infrastructure;
public class ApiKeyAttribute : ServiceFilterAttribute
{
    public ApiKeyAttribute()
        : base(typeof(ApiKeyAuthorizationFilter))
    {
    }
}