using System;
using System.Threading.Tasks;
using System.Windows;
using Regularnik.Services;
using Regularnik.ViewModels;

namespace Regularnik
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string dbPath = "Data/app.db";

            if (!StartupVerifier.IsDatabaseAvailable(dbPath, out string dbError))
            {
                MessageBox.Show(dbError, "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            Task.Run(async () =>
            {
                bool hasInternet = await StartupVerifier.IsInternetAvailableAsync();
                if (!hasInternet)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Brak połączenia z internetem.", "Błąd połączenia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    });
                }
            });

            DataContext = new MainViewModel(); // lub Twój startowy VM

            // 🚀 TEST Cohere API przy starcie
            //TestCohereApi();
        }

        //private async void TestCohereApi()
        //{
        //    try
        //    {
        //        var word = "avalanche"; // przykładowe słowo
        //        var example = await ChatGptService.GenerateExampleAsync(word);

        //        // Pełne logowanie odpowiedzi
        //        MessageBox.Show($"English: {example.English}\nPolish: {example.Polish}",
        //            "Test Cohere API", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Błąd: {ex.Message}", "Błąd Cohere API", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
    }
}