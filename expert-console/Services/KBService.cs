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

        public async Task<IEnumerable<KBViewModel>> GetKBAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"api/kb?userName={userId}");

            ValidateAuthorization(response);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<KBViewModel>>(content) ?? Enumerable.Empty<KBViewModel>();
        }

        public async Task<KBViewModel> GetKBAsync(string kbId, string userId)
        {
            var response = await _httpClient.GetAsync($"api/kb/{kbId}?userName={userId}");

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

        public async Task<IEnumerable<QuestionViewModel>> GetKBQuestionsAsync(string kbId, string userName)
        {
            var response = await _httpClient.GetAsync($"api/kb/{kbId}/questions?userName={userName}");

            ValidateAuthorization(response);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<QuestionViewModel>>(content) ?? Enumerable.Empty<QuestionViewModel>();
        }

        public async Task<QuestionViewModel> GetKBQuestionDetailAsync(string kbId, string questionId, string userName)
        {
            var response = await _httpClient.GetAsync($"api/kb/{kbId}/questions/{questionId}?userName={userName}");

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