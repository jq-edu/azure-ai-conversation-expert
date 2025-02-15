using Azure.AI.Language.QuestionAnswering;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;

namespace AgentApi.Bots
{
    public class TeamsConversationBot : TeamsActivityHandler
    {
        private ILogger<TeamsConversationBot> _logger;
        private readonly QuestionAnsweringClient _questionAnsweringClient;

        public TeamsConversationBot(QuestionAnsweringClient questionAnsweringClient, ILogger<TeamsConversationBot> logger)
        {
            _questionAnsweringClient = questionAnsweringClient;
            _logger = logger;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running TeamsConversationBot.OnMessageActivityAsync");
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text.Trim().ToLower();
            _logger.LogInformation($"Received message: {text}");

            QuestionAnsweringProject project = new QuestionAnsweringProject("hr-kb-fr", "production");
            var result = await _questionAnsweringClient.GetAnswersAsync(text, project, new AnswersOptions { Size = 1 });

            // if confidence is higher than 50% send the answer
            if (result.Value.Answers[0].Confidence > 0.7)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(result.Value.Answers[0].Answer), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Je ne sais pas répondre à cette question."), cancellationToken);
            }
        }
    }
}
