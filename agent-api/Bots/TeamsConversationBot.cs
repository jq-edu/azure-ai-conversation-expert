using System.Text.Encodings.Web;
using AgentApi.Models.Bots;
using AgentApi.Repository;
using Azure.AI.Language.QuestionAnswering;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;

namespace AgentApi.Bots
{
    public class TeamsConversationBot : TeamsActivityHandler
    {
        private ILogger<TeamsConversationBot> _logger;
        private readonly KbInfoRepository _kbRepository;
        private readonly ConversationRepository _conversationRepository;
        private readonly QuestionAnsweringClient _questionAnsweringClient;

        public TeamsConversationBot(QuestionAnsweringClient questionAnsweringClient, KbInfoRepository kbInfoRepository, ConversationRepository conversationRepository, ILogger<TeamsConversationBot> logger)
        {
            _questionAnsweringClient = questionAnsweringClient;
            _kbRepository = kbInfoRepository;
            _conversationRepository = conversationRepository;
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
                var userHistoryList = await _conversationRepository.GetItems<UserConversation>("SELECT * FROM c WHERE c.entraId = @pkey", new Dictionary<string, object> { { "@pkey", key } });

                // If there are previous conversations, check if there is an opened one.
                var openedConversation = userHistoryList.FirstOrDefault(c => c.Status == "opened");

                // if the user have a recent conversation, check if the user is asking for a new conversation
                if (openedConversation != null && openedConversation.LastActivity.AddHours(8) < DateTime.UtcNow)
                {
                    //adaptive car new question or adding to existing conversation
                    //break
                }

                // main logic to handle a new question
                await HandleQuestion(userId, text, "", turnContext, cancellationToken);
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
                UserObjectId = key,
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
            if (result.Value.Answers == null || result.Value.Answers.Count == 0 || result.Value.Answers[0].Confidence < 0.7)
            {
                openedConversation.ConversationLog.Add(DateTime.UtcNow.ToString(), new KeyValuePair<string, string>("bot", result.Value.Answers[0].Answer));
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
