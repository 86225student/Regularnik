

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Regularnik.Services
{
    public static class ChatGptServiceTest

    {
        private const string ApiKey = "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"; // 🟡 <--- Wklej tu swój faktyczny klucz API!
        private const string Endpoint = "https://api.openai.com/v1/chat/completions";

        public static async Task<string> TestApiKeyAsync()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "Jesteś asystentem." },
                    new { role = "user", content = "Hello! Test." }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(Endpoint, content);

            if ((int)response.StatusCode == 429)
            {
                return "Błąd 429: Za dużo zapytań (Too Many Requests). Poczekaj chwilę!";
            }
            if (!response.IsSuccessStatusCode)
            {
                return $"Błąd: {response.StatusCode}";
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(responseBody);

            var answer = result.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return $"✅ Odpowiedź ChatGPT: {answer}";
        }
    }
}
