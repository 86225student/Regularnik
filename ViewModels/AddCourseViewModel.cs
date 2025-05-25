using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Regularnik.Models;
using Regularnik.Services;

namespace Regularnik.ViewModels
{
    public class AddCourseViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;
        private readonly Action _onSaved;
        private readonly bool _isEditMode;
        private readonly int _editingCourseId;
        private Word _selectedWord;

        // Kurs
        private string _courseName;
        public string CourseName
        {
            get => _courseName;
            set { _courseName = value; OnPropertyChanged(); }
        }

        // Pola formularza
        private string _wordPl;
        public string WordPl
        {
            get => _wordPl;
            set { _wordPl = value; OnPropertyChanged(); }
        }

        private string _wordEn;
        public string WordEn
        {
            get => _wordEn;
            set { _wordEn = value; OnPropertyChanged(); }
        }

        private string _exPl;
        public string ExPl
        {
            get => _exPl;
            set { _exPl = value; OnPropertyChanged(); }
        }

        private string _exEn;
        public string ExEn
        {
            get => _exEn;
            set { _exEn = value; OnPropertyChanged(); }
        }

        private bool _generateExample;
        public bool GenerateExample
        {
            get => _generateExample;
            set { _generateExample = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Word> TempWords { get; } = new ObservableCollection<Word>();

        public ICommand AddWordCommand { get; }
        public ICommand SaveCourseCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand DeleteWordCommand { get; }
        public ICommand SelectWordCommand { get; }

        // Nowy kurs
        public AddCourseViewModel(DatabaseService db, Action onSaved)
        {
            _db = db;
            _onSaved = onSaved;
            _isEditMode = false;

            AddWordCommand = new RelayCommand(async _ => await AddOrUpdateWordAsync());
            SaveCourseCommand = new RelayCommand(_ => SaveCourse());
            BackCommand = new RelayCommand(_ => { if (_isEditMode) SaveCourse(); else CancelNew(); _onSaved(); });
            DeleteWordCommand = new RelayCommand(p => { if (p is Word w) TempWords.Remove(w); });
            SelectWordCommand = new RelayCommand(p => SelectWord(p as Word));
        }

        // Edycja kursu
        public AddCourseViewModel(DatabaseService db, Action onSaved, Course toEdit, IEnumerable<Word> existing)
            : this(db, onSaved)
        {
            _isEditMode = true;
            _editingCourseId = toEdit.Id;
            CourseName = toEdit.Name;
            foreach (var w in existing)
                TempWords.Add(w);
        }

        private async Task AddOrUpdateWordAsync()
        {
            if (GenerateExample && !string.IsNullOrWhiteSpace(WordEn))
            {
                try
                {
                    var example = await ChatGptService.GenerateExampleAsync(WordEn);
                    ExEn = example.English;
                    ExPl = example.Polish;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd generowania przykładowego zdania: " + ex.Message, "Błąd",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (string.IsNullOrWhiteSpace(CourseName))
            {
                MessageBox.Show("Musisz podać nazwę kursu.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(WordPl) || string.IsNullOrWhiteSpace(WordEn))
            {
                MessageBox.Show("Podaj słowo po polsku i angielsku.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (_selectedWord != null)
            {
                // **AKTUALIZUJ istniejące**
                int idx = TempWords.IndexOf(_selectedWord);
                _selectedWord.WordPl = WordPl.Trim();
                _selectedWord.WordEn = WordEn.Trim();
                _selectedWord.ExamplePl = string.IsNullOrWhiteSpace(ExPl) ? null : ExPl.Trim();
                _selectedWord.ExampleEn = string.IsNullOrWhiteSpace(ExEn) ? null : ExEn.Trim();

                // wymuś odświeżenie kolekcji:
                TempWords.RemoveAt(idx);
                TempWords.Insert(idx, _selectedWord);

                _selectedWord = null;
            }
            else
            {
                // **DODAJ nowe**
                var w = new Word
                {
                    WordPl = WordPl.Trim(),
                    WordEn = WordEn.Trim(),
                    ExamplePl = string.IsNullOrWhiteSpace(ExPl) ? null : ExPl.Trim(),
                    ExampleEn = string.IsNullOrWhiteSpace(ExEn) ? null : ExEn.Trim(),
                    Category = "NOWE",
                    CorrectCount = 0,
                    NextReviewDate = null,
                    CourseId = _isEditMode ? _editingCourseId : 0
                };
                TempWords.Add(w);
            }

            ResetEntryFields();
        }

        private void SelectWord(Word w)
        {
            if (w == null) return;
            _selectedWord = w;
            WordPl = w.WordPl;
            WordEn = w.WordEn;
            ExPl = w.ExamplePl;
            ExEn = w.ExampleEn;
        }

        private void SaveCourse()
        {
            if (string.IsNullOrWhiteSpace(CourseName) || TempWords.Count == 0)
            {
                MessageBox.Show("Uzupełnij nazwę i dodaj przynajmniej jedno słowo.", "Uwaga",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isEditMode)
            {
                _db.UpdateCourseName(_editingCourseId, CourseName.Trim());
                foreach (var w in TempWords)
                {
                    if (w.Id == 0)
                    {
                        w.CourseId = _editingCourseId;
                        _db.AddWord(w);
                    }
                    else
                    {
                        _db.UpdateWord(w);
                    }
                }
                var keep = TempWords.Where(x => x.Id > 0).Select(x => x.Id).ToList();
                _db.DeleteWordsNotInCourse(_editingCourseId, keep);
            }
            else
            {
                int newId = _db.AddCourse(CourseName.Trim());
                foreach (var w in TempWords)
                {
                    w.CourseId = newId;
                    _db.AddWord(w);
                }
            }

            _onSaved();
        }

        private void CancelNew()
        {
            TempWords.Clear();
            CourseName = WordPl = WordEn = ExPl = ExEn = string.Empty;
        }

        private void ResetEntryFields()
        {
            WordPl = WordEn = ExPl = ExEn = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
