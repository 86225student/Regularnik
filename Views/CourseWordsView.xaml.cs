using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Regularnik.Views
{
    public partial class CourseWordsView : UserControl
    {
        public CourseWordsView()
        {
            InitializeComponent();
        }

        // Po naciśnięciu Enter: odfokusowanie TextBoxa
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var scope = FocusManager.GetFocusScope((DependencyObject)sender);
                FocusManager.SetFocusedElement(scope, null);
                Keyboard.ClearFocus();
            }
        }

        // Gdy TextBox traci fokus: odfokusowanie
        private void SearchBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var scope = FocusManager.GetFocusScope((DependencyObject)sender);
            FocusManager.SetFocusedElement(scope, null);
            Keyboard.ClearFocus();
        }

        // Kliknięcie w tło UserControl: przeniesienie fokusu stamtąd
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Możemy wyczyścić aktualny fokus
            Keyboard.ClearFocus();
        }
    }
}
