namespace AgentApi.Models
{
    public class KBModel
    {
        public required string KbName { get; set; }
        public required string KbId { get; set; }
        public int OpenQuestions { get; set; }
        public DateTime LastQuestionReceived { get; set; }
    }
}