using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
            set
            {
                _isMenuVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStartVisible));
            }
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

        /* ---------- NAWIGACJA GŁÓWNA ---------- */
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
                        DataContext = new CoursesViewModel(
                            _dbService,
                            OnCourseChosen)
                    };
                    break;

                case "Statistics":
                    CurrentView = new StatisticsView
                    {
                        DataContext = new StatisticsViewModel(_dbService)
                    };
                    break;

                case "Exit":
                    Application.Current.Shutdown();
                    return;
            }

            IsMenuVisible = false;
        }

        /* ---------- OTWIERANIE WIDOKU SŁÓWEK ---------- */
        private void OnCourseSelected(Course course)
        {
            _viewStack.Push(CurrentView);
            CurrentView = new CourseWordsView
            {
                DataContext = new CourseWordsViewModel(
                    _dbService,
                    course,
                    OnCourseEdit,      // dotychczasowy „Edytuj”
                    OnCourseDeleted)   // nowy „Usuń”
            };
        }
        private void OnCourseDeleted()
        {
            // cofa do poprzedniego widoku
            GoBack();
            // jeżeli teraz jesteśmy w katalogu – odśwież listę
            if (CurrentView is CatalogView cv && cv.DataContext is CatalogViewModel vm)
                vm.LoadCourses();
        }


        /* ---------- OTWIERANIE WIDOKU DODAWANIA NOWEGO KURSU ---------- */
        private void OnAddCourse()
        {
            _viewStack.Push(CurrentView);
            CurrentView = new AddCourseView
            {
                DataContext = new AddCourseViewModel(
                    _dbService,
                    OnCourseSaved)
            };
        }

        /* ---------- OTWIERANIE WIDOKU SESJI KURSOWEJ ---------- */
        private void OnCourseChosen(Course course)
        {
            _viewStack.Push(CurrentView);
            CurrentView = new CourseSessionView
            {
                DataContext = new CourseSessionViewModel(
                    _dbService,
                    course)
            };
        }

        /* ---------- CALLBACK EDYCJI KURSU ---------- */
        private void OnCourseEdit(Course courseToEdit)
        {
            _viewStack.Push(CurrentView);

            var existingWords = _dbService.GetWords(courseToEdit.Id).ToList();

            CurrentView = new AddCourseView
            {
                DataContext = new AddCourseViewModel(
                    _dbService,
                    OnCourseEditSaved,     // <-- tu używamy nowego callbacka
                    courseToEdit,
                    existingWords)
            };
        }

        /* ---------- PO ZAPISIE EDYCJI KURSU ---------- */
        private void OnCourseEditSaved()
        {
            // Czyścimy historię, żeby wrócić od razu do katalogu
            _viewStack.Clear();

            // Odświeżamy katalog
            var catalogVm = new CatalogViewModel(
                _dbService,
                OnCourseSelected,
                OnAddCourse);
            catalogVm.LoadCourses();

            CurrentView = new CatalogView
            {
                DataContext = catalogVm
            };
        }

        /* ---------- PO ZAPISIE NOWEGO KURSU ---------- */
        private void OnCourseSaved()
        {
            // Wróć do poprzedniego widoku
            if (_viewStack.Count > 0)
                CurrentView = _viewStack.Pop();

            // Jeśli to był katalog, odśwież listę
            if (CurrentView is CatalogView cv &&
                cv.DataContext is CatalogViewModel vm)
            {
                vm.LoadCourses();
            }
        }

        /* ---------- BACK ---------- */
        private void GoBack()
        {
            if (_viewStack.Count > 0)
            {
                // Cofamy się do poprzedniego widoku
                CurrentView = _viewStack.Pop();
                IsMenuVisible = false;
            }
            else
            {
                // Już nie ma gdzie wracać – pokazujemy menu główne
                CurrentView = null;
                IsMenuVisible = true;
            }
        }


        /* ---------- ŁADOWANIE KATALOGU ---------- */
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
