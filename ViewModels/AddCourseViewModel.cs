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
    public class AddCourseViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;
        private readonly Action _onSaved;
        private readonly bool _isEditMode;
        private readonly int _editingCourseId;
        private Word _selectedWord;

        public string CourseName { get; set; }
        public string WordPl { get; set; }
        public string WordEn { get; set; }
        public string ExPl { get; set; }
        public string ExEn { get; set; }

        public ObservableCollection<Word> TempWords { get; } = new ObservableCollection<Word>();

        public ICommand AddWordCommand { get; private set; }
        public ICommand SaveCourseCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand DeleteWordCommand { get; private set; }
        public ICommand SelectWordCommand { get; private set; }

        // Konstruktor do tworzenia nowego kursu
        public AddCourseViewModel(DatabaseService db, Action onSaved)
        {
            _db = db;
            _onSaved = onSaved;
            _isEditMode = false;

            InitializeCommands();
        }

        // Konstruktor do edycji istniejącego kursu
        public AddCourseViewModel(DatabaseService db,
                                  Action onSaved,
                                  Course courseToEdit,
                                  IEnumerable<Word> existingWords)
            : this(db, onSaved)
        {
            _isEditMode = true;
            _editingCourseId = courseToEdit.Id;
            CourseName = courseToEdit.Name;
            foreach (var w in existingWords)
                TempWords.Add(w);
        }

        private void InitializeCommands()
        {
            AddWordCommand = new RelayCommand(_ =>
            {
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

                var w = new Word
                {
                    Id = _isEditMode ? 0 : 0, // nowe słowo ma Id=0
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
                ResetEntryFields();
            });

            SaveCourseCommand = new RelayCommand(_ =>
            {
                if (string.IsNullOrWhiteSpace(CourseName) || TempWords.Count == 0)
                {
                    MessageBox.Show("Uzupełnij nazwę i dodaj przynajmniej jedno słowo.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_isEditMode)
                {
                    // Aktualizacja kursu
                    _db.UpdateCourseName(_editingCourseId, CourseName.Trim());
                    // Dodaj/edytuj słowa
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
                    // Usuń słowa, które zostały usunięte w TempWords
                    var keep = TempWords.Where(w => w.Id > 0).Select(w => w.Id).ToList();
                    _db.DeleteWordsNotInCourse(_editingCourseId, keep);
                }
                else
                {
                    // Tworzenie nowego kursu
                    int newCourseId = _db.AddCourse(CourseName.Trim());
                    foreach (var w in TempWords)
                    {
                        w.CourseId = newCourseId;
                        _db.AddWord(w);
                    }
                }

                _onSaved?.Invoke();
            });

            BackCommand = new RelayCommand(_ =>
            {
                TempWords.Clear();
                CourseName = WordPl = WordEn = ExPl = ExEn = string.Empty;
                OnPropertyChanged(nameof(CourseName));
                OnPropertyChanged(nameof(WordPl));
                OnPropertyChanged(nameof(WordEn));
                OnPropertyChanged(nameof(ExPl));
                OnPropertyChanged(nameof(ExEn));
                _onSaved?.Invoke();
            });

            SelectWordCommand = new RelayCommand(param =>
            {
                _selectedWord = param as Word;
                if (_selectedWord != null)
                {
                    WordPl = _selectedWord.WordPl;
                    WordEn = _selectedWord.WordEn;
                    ExPl = _selectedWord.ExamplePl;
                    ExEn = _selectedWord.ExampleEn;
                }
            });

            DeleteWordCommand = new RelayCommand(param =>
            {
                var w = param as Word;
                if (w != null)
                    TempWords.Remove(w);
            });
        }

        private void ResetEntryFields()
        {
            WordPl = ExPl = WordEn = ExEn = string.Empty;
            OnPropertyChanged(nameof(WordPl));
            OnPropertyChanged(nameof(ExPl));
            OnPropertyChanged(nameof(WordEn));
            OnPropertyChanged(nameof(ExEn));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}