using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;

namespace AgentApi.Bots
{
    public class TeamsConversationBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text.Trim().ToLower();

            // Exemple d'appel asynchrone pour envoyer une réponse
            await turnContext.SendActivityAsync(MessageFactory.Text($"Vous avez dit : {text}"), cancellationToken);
        }
    }
}
