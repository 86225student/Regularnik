using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Regularnik.Models;
using Regularnik.Services;

namespace Regularnik.ViewModels
{
    public class CourseWordsViewModel
    {
        // Nagłówek widoku, np. "Kurs: Angielski"
        public string Header { get; }

        // Komenda podpięta pod przycisk "Edytuj kurs"
        public ICommand EditCourseCommand { get; private set; }

        // Lista słówek wyświetlana w CourseWordsView
        public ObservableCollection<Word> Words { get; } = new ObservableCollection<Word>();

        // Bieżący kurs i callback do wywołania przy edycji
        private readonly Course _course;
        private readonly Action<Course> _onEdit;

        /// <summary>
        /// Overload dla dotychczasowego wywołania (bez nawigacji) –
        /// kliknięcie "Edytuj" po prostu nic nie zrobi.
        /// </summary>
        public CourseWordsViewModel(DatabaseService db, Course course)
            : this(db, course, _ => { })
        {
        }

        /// <summary>
        /// Główny konstruktor: ładuje słowa i ustawia komendę edycji.
        /// </summary>
        /// <param name="db">Service bazy danych</param>
        /// <param name="course">Kurs do wyświetlenia</param>
        /// <param name="onEdit">Akcja wywoływana po kliknięciu "Edytuj kurs"</param>
        public CourseWordsViewModel(DatabaseService db, Course course, Action<Course> onEdit)
        {
            _course = course ?? throw new ArgumentNullException(nameof(course));
            _onEdit = onEdit ?? throw new ArgumentNullException(nameof(onEdit));

            Header = $"Kurs: {_course.Name}";

            // Załaduj słowa z bazy
            foreach (var w in db.GetWords(_course.Id))
                Words.Add(w);

            // Ustaw komendę, która wywoła przekazany callback
            EditCourseCommand = new RelayCommand(_ => _onEdit(_course));
        }
    }
}
