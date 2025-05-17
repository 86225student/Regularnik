using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Regularnik.Models;
using Regularnik.Services;
using Regularnik.Views;
using System.Diagnostics;

namespace Regularnik.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;

        /* ---------- START / MENU ---------- */
        private bool _isMenuVisible;                   // tylko kafelki
        public bool IsMenuVisible
        {
            get => _isMenuVisible;
            set { _isMenuVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsStartVisible)); }
        }

        /* ---------- OBSZAR ROBOCZY ---------- */
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBackVisible));
                OnPropertyChanged(nameof(IsStartVisible));
            }
        }

        /* ---------- Właściwości-widoczki ---------- */
        public bool IsBackVisible => CurrentView != null;                     // Wróć
        public bool IsStartVisible => !IsMenuVisible && CurrentView == null;   // Start

        /* ---------- Komendy ---------- */
        public ICommand ShowMenuCommand { get; }
        public ICommand NavigateCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand LoadCoursesCommand { get; }

        /* ---------- Data ---------- */
        public ObservableCollection<Course> Courses { get; }

        public MainViewModel()
        {
            _dbService = new DatabaseService();
            Courses = new ObservableCollection<Course>();

            ShowMenuCommand = new RelayCommand(_ =>
            {
                Debug.WriteLine("CLICK-START");
                IsMenuVisible = true;
            });

            NavigateCommand = new RelayCommand(p => Navigate(p?.ToString()));

            BackCommand = new RelayCommand(_ =>
            {
                CurrentView = null;      // zeruje widok
                IsMenuVisible = true;     // pokazuje kafelki
            });

            LoadCoursesCommand = new RelayCommand(_ => LoadCourses());
        }

        /* ---------- logika ---------- */
        private void Navigate(string destination)
        {
            switch (destination)
            {
                case "Courses": CurrentView = new CoursesView(); break;
                case "Statistics": CurrentView = new StatisticsView(); break;
                case "Catalog": CurrentView = new CatalogView(); break;
                case "Exit": Application.Current.Shutdown(); return;
            }

            IsMenuVisible = false;   // schowaj kafelki
        }

        private void LoadCourses()
        {
            Courses.Clear();
            foreach (var c in _dbService.GetCourses())
                Courses.Add(c);
        }

        /* ---------- INotifyPropertyChanged ---------- */
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
