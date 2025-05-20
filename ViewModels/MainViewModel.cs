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
        private string dest;

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
        public bool IsStartVisible => !IsMenuVisible && CurrentView == null;

        /* ---------- Komendy ---------- */
        public ICommand ShowMenuCommand { get; }
        public ICommand NavigateCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand LoadCoursesCommand { get; }

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

            BackCommand = new RelayCommand(_ =>
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
            });
        }

        /* ---------- NAWIGACJA ---------- */
        private void Navigate(string destination)
        {
            if (CurrentView != null)
                _viewStack.Push(CurrentView);       // zapamiętaj bieżący widok

            switch (destination)
            {
                case "Catalog":
                    CurrentView = new CatalogView
                    {
                        DataContext = new CatalogViewModel(
                            _dbService,
                            OnCourseSelected,   // klik istniejący kurs
                            OnAddCourse)        // ➕ Dodaj kurs
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

        /* ---------- WYBRANO KURS ---------- */
        private void OnCourseSelected(Course course)
        {
            _viewStack.Push(CurrentView);           // zapamiętaj katalog
            CurrentView = new CourseWordsView
            {
                DataContext = new CourseWordsViewModel(_dbService, course)
            };
        }

        /* ---------- KLIK „DODAJ WŁASNY KURS” ---------- */
        private void OnAddCourse()
        {
            _viewStack.Push(CurrentView);

            CurrentView = new AddCourseView
            {
                DataContext = new AddCourseViewModel(_dbService, OnCourseSaved)
            };
        }

        /* Po zapisaniu kursu w kreatorze */
        private void OnCourseSaved()
        {
            // wracamy do katalogu
            if (_viewStack.Count > 0)
                CurrentView = _viewStack.Pop();

            // odśwież listę kursów w katalogu (wywołujemy prywatne LoadCourses refleksją)
            if (CurrentView is CatalogView cv && cv.DataContext is CatalogViewModel vm)
            {
                var m = typeof(CatalogViewModel).GetMethod("LoadCourses",
                         System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                m.Invoke(vm, null);
            }
        }

        /* ---------- POBIERANIE KURSÓW (menu Start) ---------- */
        private void LoadCourses()
        {
            Courses.Clear();
            foreach (var c in _dbService.GetCourses())
                Courses.Add(c);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
