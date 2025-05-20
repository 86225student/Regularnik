using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Regularnik.ViewModels
{
    /// <summary>
    ///  Lekka baza z INotifyPropertyChanged – wystarczy do wszystkich ViewModeli.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
