using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Regularnik
{
        public partial class MainWindow : Window
        {
            public MainWindow()
            {
                InitializeComponent();
                _ = TestApiAsync();
        }
            private async Task TestApiAsync()
            {
                try
                {
                    // Pobierz wynik z ChatGPT
                    var wynik = await Regularnik.Services.ChatGptServiceTest.TestApiKeyAsync();

                    // Pokaż w oknie dialogowym
                    MessageBox.Show(wynik, "Test ChatGPT", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}", "Test ChatGPT", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

    }
}

