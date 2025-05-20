using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regularnik.Models;
using Regularnik.Services;

namespace Regularnik.ViewModels
{
    public class AddCourseViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;
        private readonly System.Action _onSaved;     // ← delegat powrotu
        private int _tempCourseId = -1;              // ID kursu w bazie

        /* --- właściwości do formularza --- */
        public string CourseName { get; set; }

        private string _wordPl;
        public string WordPl { get => _wordPl; set { _wordPl = value; OnPropertyChanged(); } }

        private string _wordEn;
        public string WordEn { get => _wordEn; set { _wordEn = value; OnPropertyChanged(); } }

        private string _exPl;
        public string ExPl { get => _exPl; set { _exPl = value; OnPropertyChanged(); } }

        private string _exEn;
        public string ExEn { get => _exEn; set { _exEn = value; OnPropertyChanged(); } }

        /* --- podgląd listy --- */
        public ObservableCollection<Word> TempWords { get; } =
            new ObservableCollection<Word>();

        public ICommand AddWordCommand { get; }
        public ICommand SaveCourseCommand { get; }

        public AddCourseViewModel(DatabaseService db, System.Action onSaved)
        {
            _db = db;
            _onSaved = onSaved;

            AddWordCommand = new RelayCommand(_ =>
            {
                if (string.IsNullOrWhiteSpace(WordPl) || string.IsNullOrWhiteSpace(WordEn))
                    return;

                /* 1) jeżeli pierwszy raz – tworzymy kurs w bazie */
                if (_tempCourseId == -1)
                {
                    if (string.IsNullOrWhiteSpace(CourseName)) return;
                    _tempCourseId = _db.AddCourse(CourseName.Trim());
                }

                /* 2) dodajemy słowo bezpośrednio do bazy */
                var w = new Word
                {
                    CourseId = _tempCourseId,
                    WordPl = WordPl.Trim(),
                    WordEn = WordEn.Trim(),
                    ExamplePl = string.IsNullOrWhiteSpace(ExPl) ? null : ExPl.Trim(),
                    ExampleEn = string.IsNullOrWhiteSpace(ExEn) ? null : ExEn.Trim()
                };
                _db.AddWord(w);

                /* 3) podgląd w UI */
                TempWords.Add(w);

                /* 4) czyść pola */
                WordPl = WordEn = ExPl = ExEn = string.Empty;
            });

            SaveCourseCommand = new RelayCommand(_ =>
            {
                /* jeśli nie dodano ani jednego słowa – nic nie rób */
                if (_tempCourseId == -1) return;

                _onSaved?.Invoke();   // wróć do katalogu
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
