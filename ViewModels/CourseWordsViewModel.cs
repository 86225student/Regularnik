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

        public ICommand EditCourseCommand { get; }
        public ICommand DeleteCourseCommand { get; }

        public bool CanModify => !_protectedNames.Contains(_course.Name);
        private static readonly string[] _protectedNames = { "A1", "A2", "B1", "B2" };

        private readonly Course _course;
        private readonly Action<Course> _onEdit;
        private readonly Action _onDelete;
        private readonly DatabaseService _db;

        // 1) Główny ctor z wszystkimi parametrami
        public CourseWordsViewModel(
            DatabaseService db,
            Course course,
            Action<Course> onEdit,
            Action onDelete)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _course = course ?? throw new ArgumentNullException(nameof(course));
            _onEdit = onEdit ?? throw new ArgumentNullException(nameof(onEdit));
            _onDelete = onDelete ?? throw new ArgumentNullException(nameof(onDelete));

            Header = $"Kurs: {_course.Name}";
            foreach (var w in _db.GetWords(_course.Id))
            {
                Words.Add(w);
                _allWords.Add(w);
            }

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
        }

        // 2) Convenience ctor bez onEdit/onDelete
        public CourseWordsViewModel(DatabaseService db, Course course)
            : this(db, course, _ => { }, () => { })
        { }

        // 3) **Nowy** ctor bez parametrów – to on usuwa błędy w XAML
        public CourseWordsViewModel()
            : this(new DatabaseService(), new Course { Id = 0, Name = "" })
        { }

        private void FilterWords()
        {
            if (string.IsNullOrWhiteSpace(_searchTerm))
            {
                Words.Clear();
                foreach (var w in _allWords) Words.Add(w);
                return;
            }

            var term = _searchTerm.Trim();
            var filtered = _allWords
                .Where(w =>
                    w.WordPl.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    w.WordEn.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            Words.Clear();
            foreach (var w in filtered) Words.Add(w);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
