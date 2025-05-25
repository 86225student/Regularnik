using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Regularnik.Models;
using Regularnik.Services;

namespace Regularnik.ViewModels
{
    public class CourseWordsViewModel
    {
        // Nagłówek widoku, np. "Kurs: Angielski"
        public string Header { get; }

        // Komendy
        public ICommand EditCourseCommand { get; private set; }
        public ICommand DeleteCourseCommand { get; private set; }

        // Kolekcja słówek
        public ObservableCollection<Word> Words { get; } = new ObservableCollection<Word>();

        private readonly Course _course;
        private readonly Action<Course> _onEdit;
        private readonly Action _onDelete;
        private readonly DatabaseService _db;

        // Konstruktor główny
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

            // Załaduj słowa
            foreach (var w in _db.GetWords(_course.Id))
                Words.Add(w);

            // Edycja
            EditCourseCommand = new RelayCommand(_ => _onEdit(_course));

            // Usunięcie kursu
            DeleteCourseCommand = new RelayCommand(_ =>
            {
                var result = MessageBox.Show(
                    $"Na pewno chcesz usunąć kurs „{_course.Name}” i wszystkie jego słowa?",
                    "Potwierdź usunięcie",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // 1) Usuń słowa
                    _db.DeleteWordsNotInCourse(_course.Id, new System.Collections.Generic.List<int>());
                    // 2) Usuń sam kurs
                    _db.DeleteCourse(_course.Id);
                    // 3) Wracamy i odświeżamy
                    _onDelete();
                }
            });
        }

        // Zachowujemy kompatybilność z dotychczasowym wywołaniem
        public CourseWordsViewModel(DatabaseService db, Course course)
            : this(db, course, _ => { }, () => { })
        {
        }
    }
}
