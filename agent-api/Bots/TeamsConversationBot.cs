using AgentApi.Models.Bots;
using AgentApi.Repository;
using Azure.AI.Language.QuestionAnswering;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Azure.Identity;
using Azure.AI.OpenAI.Chat;

namespace AgentApi.Bots
{
    public class TeamsConversationBot : TeamsActivityHandler
    {
        private ILogger<TeamsConversationBot> _logger;
        private readonly KbInfoRepository _kbRepository;
        private readonly ConversationRepository _conversationRepository;
        private readonly QuestionAnsweringClient _questionAnsweringClient;

        // public TeamsConversationBot(QuestionAnsweringClient questionAnsweringClient, KbInfoRepository kbInfoRepository, ConversationRepository conversationRepository, ILogger<TeamsConversationBot> logger)
        // {
        //     _questionAnsweringClient = questionAnsweringClient;
        //     _kbRepository = kbInfoRepository;
        //     _conversationRepository = conversationRepository;
        //     _logger = logger;
        // }

        public TeamsConversationBot(ILogger<TeamsConversationBot> logger)
        {
            _logger = logger;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running TeamsConversationBot.OnMessageActivityAsync");
            var text = turnContext.Activity.Text;
            var userId = turnContext.Activity.From.AadObjectId;
            _logger.LogDebug($"Received message: {text} from user: {userId}");

            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    // greet the user adaptive card
                    // break
                }
                else if (text.StartsWith("FEEDBACK:"))
                {
                    // todo routage
                    // break
                }

                // validate greeting

                // See if there are previous conversations saved in storage for the user
                // var userHistoryList = await _conversationRepository.GetItems<UserConversation>("SELECT * FROM c WHERE c.userObjectId = @pkey", new Dictionary<string, object> { { "@pkey", userId } });

                // If there are previous conversations, check if there is an opened one.
                // var openedConversation = userHistoryList.FirstOrDefault(c => c.Status == "opened");

                // if the user have a recent conversation, check if the user is asking for a new conversation
                //if (openedConversation != null && openedConversation.LastActivity.AddHours(8) < DateTime.UtcNow)
                //{
                //adaptive car new question or adding to existing conversation
                //break
                //}

                // main logic to handle a new question
                //await HandleQuestion(userId, text, "", turnContext, cancellationToken);

                string endpoint = "https://oai-testjq.openai.azure.com/";
                string searchEndpoint = "https://srch-test-jq.search.windows.net";
                string index = "test-index";
                string deploymentName = "gpt-4o";
                var systemMessage = "tu réponds toujours comme un cowboy et n'utilise pas d'émoticon";

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemMessage),
                    new UserChatMessage(text)
                };

                var completionOptions = new ChatCompletionOptions
                {
                    MaxOutputTokenCount=800,
                    TopP=0.95f,
                    Temperature=0.01f,
                    FrequencyPenalty=0.0f,
                    PresencePenalty=0.0f
                };

                #pragma warning disable AOAI001 // Le type est utilisé à des fins d’évaluation uniquement et est susceptible d’être modifié ou supprimé dans les futures mises à jour. Supprimez ce diagnostic pour continuer.
                var dataSources = new AzureSearchChatDataSource  
                {  
                    Endpoint = new Uri(searchEndpoint),  
                    IndexName = index,  
                    Authentication = DataSourceAuthentication.FromSystemManagedIdentity()  
                };

                completionOptions.AddDataSource(dataSources);
                #pragma warning restore AOAI001 // Le type est utilisé à des fins d’évaluation uniquement et est susceptible d’être modifié ou supprimé dans les futures mises à jour. Supprimez ce diagnostic pour continuer.
            
                var azureClient = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
                var chatClient = azureClient.GetChatClient(deploymentName);

                var result = await chatClient.CompleteChatAsync(messages, completionOptions);
                var response = result.Value.Content[0].Text;
                Console.WriteLine($"{result.Value.Role}: {response}");
                await turnContext.SendActivityAsync(MessageFactory.Text(response), cancellationToken);
            }
            catch (Exception e)
            {
                // Inform the user an error occurred.
                _logger.LogError($"Error reading from storage: {e.Message}");
                _logger.LogError(e.StackTrace);
                await turnContext.SendActivityAsync("Sorry, something went wrong reading your stored messages!");
            }
        }

        private async Task HandleQuestion(string userId, string text, string kbId, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            UserConversation openedConversation = new UserConversation
            {
                Id = Guid.NewGuid().ToString(),
                KbId = kbId,
                UserObjectId = userId,
                Name = turnContext.Activity.From.Name,
                StartTime = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                Status = "opened",
                ConversationLog = new Dictionary<string, KeyValuePair<string, string>>()
            };
            openedConversation.ConversationLog.Add(DateTime.UtcNow.ToString(), new KeyValuePair<string, string>("user", text));

            QuestionAnsweringProject project = new QuestionAnsweringProject("hr-kb-fr", "production");
            var result = await _questionAnsweringClient.GetAnswersAsync(text, project, new AnswersOptions { Size = 1 });

            // check if answer is null or empty or low threshold
            var answers = result.Value.Answers;
            if (answers == null || answers.Count == 0 || answers[0].Confidence < 0.7)
            {
                openedConversation.ConversationLog.Add(DateTime.UtcNow.ToString(), new KeyValuePair<string, string>("bot", "no good answer found in kb"));
                openedConversation.Status = "waiting-exert";
                await turnContext.SendActivityAsync(MessageFactory.Text("Je suis en apprentissage constant et ne suis pas certain de la réponse. Je vous reviens sous peu."), cancellationToken);
            }
            else
            {
                openedConversation.ConversationLog.Add(DateTime.UtcNow.ToString(), new KeyValuePair<string, string>("bot", result.Value.Answers[0].Answer));
                openedConversation.Status = "answered-kb";
                await turnContext.SendActivityAsync(MessageFactory.Text(result.Value.Answers[0].Answer), cancellationToken);
                //ask feedback adaptive card
            }
            // Save the new conversation to storage.
            await _conversationRepository.InsertAsync(openedConversation);
        }
    }
}
