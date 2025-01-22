using System.Text.Json.Serialization;

namespace ExpertConsole.Models
{
    public class UserViewModel
    {
        [JsonPropertyName("userId")]
        public required string UserId { get; set; }
        [JsonPropertyName("userPrincipalName")]
        public required string UserPrincipalName { get; set; }
        [JsonPropertyName("displayName")]
        public required string DisplayName { get; set; }
    }
}