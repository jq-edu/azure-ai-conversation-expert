namespace AgentApi.Models
{
    public class QuestionModel
    {
        public required string QuestionId { get; set; }
        public required string Question { get; set; }
        public required string Demandeur { get; set; }
        public required string Status { get; set; }
        public DateTime DateDemande { get; set; }
        public string? Reponse { get; set; }
    }
}