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

namespace Regularnik.ViewModels
{
    public class CourseWordsViewModel : INotifyPropertyChanged
    {
        public string Header { get; }
        public ObservableCollection<Word> Words { get; } = new ObservableCollection<Word>();
        private readonly List<Word> _allWords = new List<Word>();

        private readonly Course _course;
        private readonly DatabaseService _db;
        private readonly Action<Course> _onEdit;
        private readonly Action _onDelete;

        // sortowanie
        private bool _isSortedAlphabetically = false;
        public string SortButtonText =>
            _isSortedAlphabetically ? "Sortuj domyślnie" : "Sortuj alfabetycznie";

        // komendy
        public ICommand EditCourseCommand { get; }
        public ICommand DeleteCourseCommand { get; }
        public ICommand ToggleSortCommand { get; }

        public bool CanModify => !_protectedNames.Contains(_course.Name);
        private static readonly string[] _protectedNames = { "A1", "A2", "B1", "B2" };

        public CourseWordsViewModel(DatabaseService db, Course course, Action<Course> onEdit, Action onDelete)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _course = course ?? throw new ArgumentNullException(nameof(course));
            _onEdit = onEdit ?? throw new ArgumentNullException(nameof(onEdit));
            _onDelete = onDelete;

            Header = $"Kurs: {_course.Name}";

            // komendy
            EditCourseCommand = new RelayCommand(_ => _onEdit(_course), _ => CanModify);
            DeleteCourseCommand = new RelayCommand(_ =>
            {
                var result = MessageBox.Show(
                    $"Na pewno usunąć kurs „{_course.Name}” i wszystkie jego słowa?",
                    "Potwierdź usunięcie",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _db.DeleteWordsNotInCourse(_course.Id, new List<int>());
                    _db.DeleteCourse(_course.Id);
                    _onDelete();
                }
            }, _ => CanModify);

            ToggleSortCommand = new RelayCommand(_ =>
            {
                _isSortedAlphabetically = !_isSortedAlphabetically;
                OnPropertyChanged(nameof(SortButtonText));
                LoadWordsFromDatabase();
            }, _ => true);

            // pierwsze wczytanie
            LoadWordsFromDatabase();
        }

        // convenience ctor dla XAML
        public CourseWordsViewModel()
            : this(new DatabaseService(),
                   new Course { Id = 0, Name = "" },
                   _ => { },
                   () => { })
        { }

        private string _searchTerm;
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm == value) return;
                _searchTerm = value;
                OnPropertyChanged();
                FilterWords();
            }
        }

        private void LoadWordsFromDatabase()
        {
            _allWords.Clear();
            IEnumerable<Word> source = _isSortedAlphabetically
                ? _db.GetWordsSortedAlphabetically(_course.Id)
                : _db.GetWords(_course.Id);

            foreach (var w in source)
                _allWords.Add(w);

            FilterWords();
        }

        // Poprawiona metoda filtracji: używamy IndexOf zamiast Contains(string, StringComparison)
        private void FilterWords()
        {
            Words.Clear();
            var list = string.IsNullOrWhiteSpace(_searchTerm)
                ? _allWords
                : _allWords.Where(w =>
                    w.WordPl.IndexOf(_searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    w.WordEn.IndexOf(_searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var w in list)
                Words.Add(w);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
