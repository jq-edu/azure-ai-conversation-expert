using AgentApi.Bots;
using AgentApi.Infrastructure;
using AgentApi.Infrastructure.ApiKey;
using AgentApi.Infrastructure.Bot;
using AgentApi.Repository;
using Azure.AI.Language.QuestionAnswering;
using Azure.Identity;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication();

// manage API keys - used from https://www.milanjovanovic.tech/blog/how-to-implement-api-key-authentication-in-aspnet-core
builder.Services.AddSingleton<ApiKeyAuthorizationFilter>();
builder.Services.AddSingleton<IApiKeyValidator, ApiKeyValidator>(provider => new ApiKeyValidator("123"));

// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>(provider =>
{
    var configuration = builder.Configuration.GetSection("BotFramework");
    return new ConfigurationBotFrameworkAuthentication(configuration);
});

// Create the Bot Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, TeamsConversationBot>();

// Add the ConversationRepository as a singleton
// builder.Services.AddSingleton<ConversationRepository>(q =>
// {
//     var options = builder.Configuration.GetSection("CosmosDb").Get<CosmosConfiguration>();
//     if (options == null)
//     {
//         throw new ArgumentNullException(nameof(options), "The CosmosDb configuration is missing or empty.");
//     }
//     var conversationContainerId = builder.Configuration.GetValue<string>("CosmosDb:ConversationContainerId");
//     if (string.IsNullOrEmpty(conversationContainerId))
//     {
//         throw new ArgumentNullException(nameof(conversationContainerId), "The CosmosDb:ConversationContainerId configuration value is missing or empty.");
//     }
//     options.ContainerId = conversationContainerId;

//     return new ConversationRepository(options, new DefaultAzureCredential());
// });

// // Add the KbInfoRepository as a singleton
// builder.Services.AddSingleton<KbInfoRepository>(q =>
// {
//     var options = builder.Configuration.GetSection("CosmosDb").Get<CosmosConfiguration>();
//     if (options == null)
//     {
//         throw new ArgumentNullException(nameof(options), "The CosmosDb configuration is missing or empty.");
//     }
//     var kbContainerId = builder.Configuration.GetValue<string>("CosmosDb:KbContainerId");
//     if (string.IsNullOrEmpty(kbContainerId))
//     {
//         throw new ArgumentNullException(nameof(kbContainerId), "The CosmosDb:KbContainerId configuration value is missing or empty.");
//     }
//     options.ContainerId = kbContainerId;

//     return new KbInfoRepository(options, new DefaultAzureCredential());
// });

// // Add the QuestionAnsweringClient as a singleton
// builder.Services.AddSingleton<QuestionAnsweringClient>(q =>
// {
//     var baseUrl = builder.Configuration.GetValue<string>("AzureAI:QuestionAnsweringBaseUrl");
//     if (string.IsNullOrEmpty(baseUrl))
//     {
//         throw new ArgumentNullException(nameof(baseUrl), "The AzureAI:QuestionAnsweringBaseUrl configuration value is missing or empty.");
//     }

//     return new QuestionAnsweringClient(new Uri(baseUrl), new DefaultAzureCredential());
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();