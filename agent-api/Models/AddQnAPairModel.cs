namespace AgentApi.Models
{
    public class AddQnAPairModel
    {
        public required string QuestionId { get; set; }
        public required string Question { get; set; }
        public required string Reponse { get; set; }
    }
}