using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Regularnik.Models;
using Regularnik.Services;
using Regularnik.Views;

namespace Regularnik.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService = new DatabaseService();
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

        /* ---------- Widoczki ---------- */
        public bool IsBackVisible => CurrentView != null;
        public bool IsStartVisible => !IsMenuVisible && CurrentView == null;

        /* ---------- Komendy ---------- */
        public ICommand ShowMenuCommand { get; }
        public ICommand NavigateCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand LoadCoursesCommand { get; }

        /* ---------- Dane ---------- */
        public ObservableCollection<Course> Courses { get; } = new ObservableCollection<Course>();

        public MainViewModel()
        {
            ShowMenuCommand = new RelayCommand(_ => IsMenuVisible = true);
            NavigateCommand = new RelayCommand(p => Navigate(p?.ToString()));
            BackCommand = new RelayCommand(_ => GoBack());
            LoadCoursesCommand = new RelayCommand(_ => LoadCourses());
        }

        /* ---------- nawigacja ---------- */
        private void Navigate(string dest)
        {
            if (CurrentView != null)
                _viewStack.Push(CurrentView);

            switch (dest)
            {
                case "Catalog":
                    CurrentView = new CatalogView
                    {
                        DataContext = new CatalogViewModel(_dbService, OnCourseSelected)
                    };
                    break;

                case "Courses":
                    CurrentView = new CoursesView
                    {
                        DataContext = new CoursesViewModel(_dbService, OnCourseChosen)
                    };
                    break;

                case "Statistics":
                    CurrentView = new StatisticsView();
                    break;

                case "Exit":
                    Application.Current.Shutdown();
                    return;
            }

            IsMenuVisible = false;
        }

        /* —— klik w katalogu —— */
        private void OnCourseSelected(Course course)
        {
            _viewStack.Push(CurrentView);
            CurrentView = new CourseWordsView
            {
                DataContext = new CourseWordsViewModel(_dbService, course)
            };
        }

        /* —— klik w „Kursy” —— */
        private void OnCourseChosen(Course c)
        {
            _viewStack.Push(CurrentView);
            CurrentView = new CourseSessionView
            {
                DataContext = new CourseSessionViewModel(_dbService, c)
            };
        }

        /* ---------- cofanie ---------- */
        private void GoBack()
        {
            if (_viewStack.Count > 0)
            {
                CurrentView = _viewStack.Pop();
                IsMenuVisible = false;
            }
            else
            {
                CurrentView = null;
                IsMenuVisible = true;
            }
        }

        /* ---------- kursy ---------- */
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
