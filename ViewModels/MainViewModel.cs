using System;
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
        private readonly DatabaseService _dbService;
        private readonly Stack<object> _viewStack = new Stack<object>();

        public MainViewModel()
        {
            _dbService = new DatabaseService();
            Courses = new ObservableCollection<Course>();

            ShowMenuCommand = new RelayCommand(_ => IsMenuVisible = true);
            NavigateCommand = new RelayCommand(p => Navigate(p?.ToString()));
            BackCommand = new RelayCommand(_ => GoBack());
            LoadCoursesCommand = new RelayCommand(_ => LoadCourses());

            // start
            IsMenuVisible = false;
        }

        /* ---------- START / MENU ---------- */
        private bool _isMenuVisible;
        public bool IsMenuVisible
        {
            get => _isMenuVisible;
            set { _isMenuVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsStartVisible)); }
        }

        public bool IsStartVisible => !IsMenuVisible && CurrentView == null;

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

        public bool IsBackVisible => CurrentView != null;

        /* ---------- KOMENDY ---------- */
        public ICommand ShowMenuCommand { get; }
        public ICommand NavigateCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand LoadCoursesCommand { get; }

        /* ---------- DANE MENU START ---------- */
        public ObservableCollection<Course> Courses { get; }

        /* ---------- NAWIGACJA ---------- */
        private void Navigate(string destination)
        {
            if (CurrentView != null)
                _viewStack.Push(CurrentView);

            switch (destination)
            {
                case "Catalog":
                    CurrentView = new CatalogView
                    {
                        DataContext = new CatalogViewModel(
                            _dbService,
                            OnCourseSelected,
                            OnAddCourse)
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

        private void OnCourseSelected(Course course)
        {
            _viewStack.Push(CurrentView);
            CurrentView = new CourseWordsView
            {
                DataContext = new CourseWordsViewModel(_dbService, course)
            };
        }

        private void OnAddCourse()
        {
            _viewStack.Push(CurrentView);
            CurrentView = new AddCourseView
            {
                DataContext = new AddCourseViewModel(_dbService, OnCourseSaved)
            };
        }

        private void OnCourseChosen(Course c)
        {
            _viewStack.Push(CurrentView);
            CurrentView = new CourseSessionView
            {
                DataContext = new CourseSessionViewModel(_dbService, c)
            };
        }

        private void OnCourseSaved()
        {
            // wracamy do poprzedniego widoku (katalog)
            if (_viewStack.Count > 0)
                CurrentView = _viewStack.Pop();

            // odśwież katalog
            if (CurrentView is CatalogView cv && cv.DataContext is CatalogViewModel vm)
            {
                var mi = typeof(CatalogViewModel).GetMethod(
                    "LoadCourses",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                mi?.Invoke(vm, null);
            }
        }

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
