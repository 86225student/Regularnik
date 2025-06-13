using Regularnik.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Regularnik.Views
{
    public partial class AddCourseView : UserControl
    {
        public AddCourseView()
        {
            InitializeComponent();
        }
        //private void BackButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (DataContext is AddCourseViewModel vm)
        //    {
        //        if (vm.IsDirty)
        //        {
        //            var result = MessageBox.Show(
        //                "Posiadasz niezatwierdzone zmiany. Czy chcesz je zapisać przed wyjściem do poprzedniego ekranu?",
        //                "Niezapisane zmiany",
        //                MessageBoxButton.YesNoCancel,
        //                MessageBoxImage.Warning);

        //            if (result == MessageBoxResult.Yes)
        //            {
        //                vm.SaveCourseCommand.Execute(null);
        //            }
        //            else if (result == MessageBoxResult.No)
        //            {
        //                vm.IsDirty = false; // wymuszenie pozwolenia na wyjście
        //                vm.BackCommand.Execute(null);
        //            }
        //            // jeśli Cancel – nic nie rób
        //        }
        //        else
        //        {
        //            vm.BackCommand.Execute(null);
        //        }
        //    }
        //}

    }
}
