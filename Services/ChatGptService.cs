using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Regularnik.Services
{
    public static class ChatGptService
    {
        private const string ApiKey = "3abZ3aKZRdH2Oon6wY4r1TiUkNJwEvvaiQ6iHASW"; // Twój klucz API Cohere
        private const string Endpoint = "https://api.cohere.ai/v1/chat";

        public static async Task<(string English, string Polish)> GenerateExampleAsync(string wordEn)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
            client.DefaultRequestHeaders.Add("Cohere-Version", "2022-12-06"); // Wymagane przez Cohere

            var prompt = $"Napisz JEDNO i tylko jedno przykładowe zdanie po angielsku ze słowem '{wordEn}' oraz jego tłumaczenie na polski. Nie podawaj więcej niż jednego przykładu.";

            var requestBody = new
            {
                model = "command-r", // domyślny model w Cohere
                chat_history = new[]
                {
                    new { user_name = "system", text = "Jesteś tłumaczem angielsko-polskim." }
                },
                message = prompt
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
                        await Task.Delay(3000); // poczekaj 3 sekundy przed kolejną próbą
                        continue;
                    }
                    throw new Exception("Limit zapytań do API został przekroczony (429). Spróbuj ponownie później.");
                }

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonDocument.Parse(responseBody);

                // W Cohere odpowiedź jest w polu 'text'
                var answer = result.RootElement
                                    .GetProperty("text")
                                    .GetString();

                MessageBox.Show($"Pełna odpowiedź tekstowa od Cohere:\n{answer}", "Odpowiedź API", MessageBoxButton.OK, MessageBoxImage.Information);

                // 🔵 Teraz przyjmujemy, że odpowiedź jest w 2 liniach: 1) angielska, 2) polska
                var lines = answer.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                var en = lines.Length > 0 ? lines[0].Trim(' ', '"', '.', '\n') : "Brak przykładu";
                if (!string.IsNullOrWhiteSpace(wordEn) && en.IndexOf(wordEn, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    en = Regex.Replace(en, Regex.Escape(wordEn), "...", RegexOptions.IgnoreCase);
                }
                var pl = lines.Length > 1 ? lines[1].Trim(' ', '"', '.', '\n') : "Brak tłumaczenia";
                MessageBox.Show($"English: {en}\nPolish: {pl}",
                    "Test Cohere API", MessageBoxButton.OK, MessageBoxImage.Information);

                return (en, pl);
            }

            throw new Exception("Nie udało się po 3 próbach.");
        }
    }
}
