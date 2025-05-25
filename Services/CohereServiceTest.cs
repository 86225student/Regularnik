using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Regularnik.Services
{
    public static class CohereServiceTest
    {
        private const string ApiKey = "3abZ3aKZRdH2Oon6wY4r1TiUkNJwEvvaiQ6iHASW"; // Wklej tu swój klucz Cohere!
        private const string Endpoint = "https://api.cohere.ai/v1/chat";

        public static async Task<string> TestApiKeyAsync()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
            client.DefaultRequestHeaders.Add("Cohere-Version", "2022-12-06"); // Wymagane przez Cohere

            var requestBody = new
            {
                model = "command-r", // Albo inny model Cohere
                chat_history = new[]
                {
                    new { user_name = "system", text = "Jesteś asystentem." }
                },
                message = "Hello! Test."
            };

            var json = JsonSerializer.Serialize(requestBody);

            // 3 próby
            for (int i = 0; i < 3; i++)
            {
                using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(Endpoint, content);

                if ((int)response.StatusCode == 429)
                {
                    if (i < 2)
                    {
                        await Task.Delay(3000);
                        continue;
                    }
                    return "Błąd 429: Za dużo zapytań (Too Many Requests). Poczekaj chwilę!";
                }

                if (!response.IsSuccessStatusCode)
                {
                    return $"Błąd: {response.StatusCode}";
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonDocument.Parse(responseBody);

                // Cohere zwraca tekst w polu 'text'
                var answer = result.RootElement
                    .GetProperty("text")
                    .GetString();

                return $"✅ Odpowiedź Cohere: {answer}";
            }

            return "Nie udało się po 3 próbach.";
        }
    }
}
