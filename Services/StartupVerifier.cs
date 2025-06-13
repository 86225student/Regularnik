using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Regularnik.Services
{
    public static class StartupVerifier
    {
        public static bool IsDatabaseAvailable(string dbPath, out string message)
        {
            message = "";

            if (!File.Exists(dbPath))
            {
                message = $"Nie znaleziono pliku bazy danych pod ścieżką: {dbPath}";
                return false;
            }

            try
            {
                using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
                conn.Open();

                // Testowa kwerenda — sprawdź np. istnienie tabeli `words`
                using var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='words'", conn);
                var result = cmd.ExecuteScalar();

                if (result == null)
                {
                    message = "Baza danych nie zawiera wymaganej tabeli 'words'.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                message = $"Nie udało się otworzyć bazy danych: {ex.Message}";
                return false;
            }
        }
        public static async Task<bool> IsInternetAvailableAsync(string testUrl = "https://www.google.com")
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = await client.GetAsync(testUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

    }

}
