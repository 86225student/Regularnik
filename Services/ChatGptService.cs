using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Regularnik.Services
{
    public static class ChatGptService
    {
        private const string ApiKey = "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"; // Twój klucz API
        private const string Endpoint = "https://api.openai.com/v1/chat/completions";

        public static async Task<(string English, string Polish)> GenerateExampleAsync(string wordEn)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);

            var prompt = $"Napisz przykładowe zdanie po angielsku ze słowem '{wordEn}' i jego tłumaczenie na polski.";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "Jesteś tłumaczem angielsko-polskim." },
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(Endpoint, content);

            if ((int)response.StatusCode == 429)
            {
                throw new Exception("Limit zapytań do API został przekroczony (429). Spróbuj ponownie później.");
            }

            response.EnsureSuccessStatusCode();


            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(responseBody);

            var answer = result.RootElement
                                .GetProperty("choices")[0]
                                .GetProperty("message")
                                .GetProperty("content")
                                .GetString();

            // Zakładamy, że API zwraca coś w stylu: "English: ...\nPolish: ..."
            var lines = answer.Split('\n');
            var en = lines.FirstOrDefault(l => l.StartsWith("English:", StringComparison.OrdinalIgnoreCase))?.Substring(8).Trim();
            var pl = lines.FirstOrDefault(l => l.StartsWith("Polish:", StringComparison.OrdinalIgnoreCase))?.Substring(7).Trim();

            return (en ?? "Brak przykładu", pl ?? "Brak tłumaczenia");
        }
    }
}




//using System;
//using System.Linq;
//using System.Net.Http;
//using System.Threading.Tasks;

//namespace Regularnik.Services;

//public static class ChatGptService
//{
//    private const string ApiKey = "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";
//    private const string Endpoint = "https://api.openai.com/v1/chat/completions";

//    public static async Task<(string English, string Polish)> GenerateExampleAsync(string wordEn)
//    {
//        using var client = new HttpClient();
//        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);

//        var prompt = $"Napisz przykładowe zdanie po angielsku używające słowa '{wordEn}', oraz jego tłumaczenie na polski.";

//        var requestBody = new
//        {
//            model = "gpt-3.5-turbo",
//            messages = new[]
//            {
//                new { role = "system", content = "Jesteś tłumaczem angielsko-polskim." },
//                new { role = "user", content = prompt }
//            }
//        };

//        var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
//        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

//        var response = await client.PostAsync(Endpoint, content);
//        response.EnsureSuccessStatusCode();

//        var responseBody = await response.Content.ReadAsStringAsync();
//        var result = System.Text.Json.JsonDocument.Parse(responseBody);

//        var answer = result.RootElement
//                            .GetProperty("choices")[0]
//                            .GetProperty("message")
//                            .GetProperty("content")
//                            .GetString();

//        // Zakładamy, że API zwraca coś w stylu: "English: ...\nPolish: ..."
//        var lines = answer.Split('\n');
//        var en = lines.FirstOrDefault(l => l.StartsWith("English:", StringComparison.OrdinalIgnoreCase))?.Substring(8).Trim();
//        var pl = lines.FirstOrDefault(l => l.StartsWith("Polish:", StringComparison.OrdinalIgnoreCase))?.Substring(7).Trim();

//        return (en ?? "Brak przykładu", pl ?? "Brak tłumaczenia");
//    }
//}
