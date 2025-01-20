using System.Text.Json.Serialization;

namespace ExpertConsole.Models
{
    public class KBViewModel
    {
        [JsonPropertyName("kbName")]
        public required string KbName { get; set; }
        [JsonPropertyName("kbId")]
        public required string KbId { get; set; }
        [JsonPropertyName("openQuestions")]
        public int OpenQuestions { get; set; }
        [JsonPropertyName("lastQuestionReceived")]
        public DateTime LastQuestionReceived { get; set; }
    }
}