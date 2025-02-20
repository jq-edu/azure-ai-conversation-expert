using System.Text.Json.Serialization;

namespace AgentApi.Models.Bots
{
    public class UserConversation
    {
        public required string Id { get; set; }
        public required string KbId { get; set; }
        public required string UserObjectId { get; set; }
        public required string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastActivity { get; set; }
        public required string Status { get; set; }
        public required Dictionary<string, KeyValuePair<string, string>> ConversationLog { get; set; }
    }
} 