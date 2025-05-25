using System;
using System.Threading.Tasks;
using System.Windows;
using Regularnik.Services;

namespace Regularnik
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

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