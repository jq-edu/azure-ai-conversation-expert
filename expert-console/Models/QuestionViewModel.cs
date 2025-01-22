using System.Text.Json.Serialization;

namespace ExpertConsole.Models
{
    public class QuestionViewModel
    {
        [JsonPropertyName("questionId")]
        public required string QuestionId { get; set; }
        [JsonPropertyName("question")]
        public required string Question { get; set; }
        [JsonPropertyName("demandeur")]
        public required string Demandeur { get; set; }
        [JsonPropertyName("status")]
        public required string Status { get; set; }
        [JsonPropertyName("dateDemande")]
        public DateTime DateDemande { get; set; }
        [JsonPropertyName("reponse")]
        public string? Reponse { get; set; }
        [JsonPropertyName("dateReponse")]
        public DateTime? DateReponse { get; set; }
    }
}