using System.Net;
using System.Text.Json;
using ExpertConsole.Models;

namespace ExpertConsole.Services
{
    public class KBService
    {
        private readonly HttpClient _httpClient;

        public KBService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("agent-api");
        }

        public async Task<IEnumerable<KBViewModel>> GetKBAsync(string expertId)
        {
            var response = await _httpClient.GetAsync($"api/kb?expertId={expertId}");

            ValidateAuthorization(response);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<KBViewModel>>(content) ?? Enumerable.Empty<KBViewModel>();
        }

        public async Task<KBViewModel> GetKBAsync(string kbId, string expertId)
        {
            var response = await _httpClient.GetAsync($"api/kb/{kbId}?expertId={expertId}");

            ValidateAuthorization(response);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
            {
                throw new KeyNotFoundException($"Knowledge base with ID {kbId} not found.");
            }

            var kbViewModel = JsonSerializer.Deserialize<KBViewModel>(content);
            if (kbViewModel == null)
            {
                throw new InvalidOperationException("Deserialization of KBViewModel returned null.");
            }
            return kbViewModel;
        }

        public async Task<IEnumerable<QuestionViewModel>> GetKBQuestionsAsync(string kbId, string expertId)
        {
            var response = await _httpClient.GetAsync($"api/kb/{kbId}/questions?expertId={expertId}");

            ValidateAuthorization(response);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<QuestionViewModel>>(content) ?? Enumerable.Empty<QuestionViewModel>();
        }

        public async Task<QuestionViewModel> GetKBQuestionDetailAsync(string kbId, string questionId, string expertId)
        {
            var response = await _httpClient.GetAsync($"api/kb/{kbId}/questions/{questionId}?expertId={expertId}");

            ValidateAuthorization(response);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
            {
                throw new KeyNotFoundException($"Question with ID {questionId} not found in knowledge base with ID {kbId}.");
            }

            var questionViewModel = JsonSerializer.Deserialize<QuestionViewModel>(content);
            if (questionViewModel == null)
            {
                throw new InvalidOperationException("Deserialization of QuestionViewModel returned null.");
            }
            return questionViewModel;
        }

        public async Task<IEnumerable<QuestionViewModel>> GetUserQuestionsHistoryAsync(string requesterId, string kbId, string expertId)
        {
            var response = await _httpClient.GetAsync($"api/user/{requesterId}/history?expertId={expertId}&kbId={kbId}");

            ValidateAuthorization(response);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<QuestionViewModel>>(content) ?? Enumerable.Empty<QuestionViewModel>();
        }

        public async Task AddQnAPairAsync(string kbId, string questionId, string question, string reponse, string userPrincipalName)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/kb/{kbId}/addQnAPair?expertId={userPrincipalName}",
                new { questionId, question, reponse });
            
            ValidateAuthorization(response);

            response.EnsureSuccessStatusCode();
        }

        private void ValidateAuthorization(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                // User is not authorized to access the KB API.
                throw new UnauthorizedAccessException();
            }
        }
    }
}