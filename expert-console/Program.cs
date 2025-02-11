using ExpertConsole.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph()
    .AddDistributedTokenCaches();

// Add the HttpClient with the required services to connect to the KB API.
builder.Services.AddHttpClient("agent-api", httpClient =>
{
    var baseUrl = builder.Configuration.GetSection("AgentApi:BaseUrl").Get<string>();
    if (string.IsNullOrEmpty(baseUrl))
    {
        throw new ArgumentNullException(nameof(baseUrl), "Base URL for Agent API is not configured.");
    }

    httpClient.BaseAddress = new Uri(baseUrl);

    // The GitHub API requires two headers.
    httpClient.DefaultRequestHeaders.Add(
        HeaderNames.Accept, "application/json");
    httpClient.DefaultRequestHeaders.Add(
        "X-Api-Key", builder.Configuration.GetSection("AgentApi:ApiKey").Get<string>());
});

builder.Services.AddScoped<KBService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandler = ctx => {
        var feature = ctx.Features.Get<IExceptionHandlerFeature>();
        if (feature?.Error is MsalUiRequiredException
            or { InnerException: MsalUiRequiredException }
            or { InnerException.InnerException: MsalUiRequiredException })
        {
            ctx.Response.Cookies.Delete($"{CookieAuthenticationDefaults.CookiePrefix}{CookieAuthenticationDefaults.AuthenticationScheme}");
            ctx.Response.Redirect(ctx.Request.GetEncodedPathAndQuery());
        }
        return Task.CompletedTask;
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
