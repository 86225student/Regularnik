using System.Collections.ObjectModel;
using System.Collections.Generic;          //  ←  stos
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
        private readonly Stack<object> _viewStack = new Stack<object>();

        /* ---------- START / MENU ---------- */
        private bool _isMenuVisible;
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
        public bool IsBackVisible => CurrentView != null;
        public bool IsStartVisible => !IsMenuVisible && CurrentView == null;

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

            ShowMenuCommand = new RelayCommand(_ => IsMenuVisible = true);

            NavigateCommand = new RelayCommand(p => Navigate(p?.ToString()));

            BackCommand = new RelayCommand(_ =>
            {
                if (_viewStack.Count > 0)
                {
                    CurrentView = _viewStack.Pop(); // cofamy o jeden poziom
                    IsMenuVisible = false;          // zostajemy poza menu
                }
                else
                {
                    CurrentView = null;             // wróć do menu Start
                    IsMenuVisible = true;
                }
            });

            LoadCoursesCommand = new RelayCommand(_ => LoadCourses());
        }

        /* ---------- logika ---------- */
        private void Navigate(string destination)
        {
            if (CurrentView != null)
                _viewStack.Push(CurrentView);       // zapamiętaj bieżący widok

            switch (destination)
            {
                case "Catalog":
                    CurrentView = new CatalogView
                    {
                        DataContext = new CatalogViewModel(_dbService, OnCourseSelected)
                    };
                    break;

                case "Courses":
                    CurrentView = new CoursesView();
                    break;

                case "Statistics":
                    CurrentView = new StatisticsView();
                    break;

                case "Exit":
                    Application.Current.Shutdown();
                    return;
            }

            IsMenuVisible = false; // chowamy główne menu
        }

        /* —— kliknięto kurs —— */
        private void OnCourseSelected(Course course)
        {
            _viewStack.Push(CurrentView);           // zapamiętaj katalog
            CurrentView = new CourseWordsView
            {
                DataContext = new CourseWordsViewModel(_dbService, course)
            };
        }

        /* ---------- pobranie kursów ---------- */
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
