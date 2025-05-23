using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public string CourseName { get; set; }

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

        public ObservableCollection<Word> TempWords { get; } =
            new ObservableCollection<Word>();

        public ICommand AddWordCommand { get; }
        public ICommand SaveCourseCommand { get; }
        public ICommand BackCommand { get; }

        public AddCourseViewModel(DatabaseService db, Action onSaved)
        {
            _db = db;
            _onSaved = onSaved;

            // Komenda dodająca słowo tylko do pamięci
            AddWordCommand = new RelayCommand(_ =>
            {
                if (string.IsNullOrWhiteSpace(CourseName))
                {
                    MessageBox.Show("Musisz podać nazwę kursu.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(WordPl) || string.IsNullOrWhiteSpace(WordEn))
                {
                    MessageBox.Show("Podaj słowo po polsku i po angielsku.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var w = new Word
                {
                    WordPl = WordPl.Trim(),
                    WordEn = WordEn.Trim(),
                    ExamplePl = string.IsNullOrWhiteSpace(ExPl) ? null : ExPl.Trim(),
                    ExampleEn = string.IsNullOrWhiteSpace(ExEn) ? null : ExEn.Trim(),
                    Category = "NOWE",
                    CorrectCount = 0,
                    NextReviewDate = null
                };

                TempWords.Add(w);

                // Reset pól formularza
                WordPl = ExPl = WordEn = ExEn = string.Empty;
            });

            // Komenda zapisująca kurs i słowa do bazy
            SaveCourseCommand = new RelayCommand(_ =>
            {
                if (TempWords.Count == 0)
                {
                    MessageBox.Show("Dodaj przynajmniej jedno słowo.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 1) utwórz kurs w bazie
                var trimmedName = CourseName.Trim();
                int newCourseId = _db.AddCourse(trimmedName);

                // 2) przypisz CourseId i zapisz każde słowo
                foreach (var w in TempWords)
                {
                    w.CourseId = newCourseId;
                    _db.AddWord(w);
                }

                // 3) wykonaj callback nawigacji
                _onSaved?.Invoke();
            });

            // Komenda anulująca i czyszcząca dane
            BackCommand = new RelayCommand(_ =>
            {
                // Wyczyść słowa z pamięci
                TempWords.Clear();

                // Wyczyść pola formularza
                CourseName = WordPl = WordEn = ExPl = ExEn = string.Empty;
                OnPropertyChanged(nameof(CourseName));
                OnPropertyChanged(nameof(WordPl));
                OnPropertyChanged(nameof(WordEn));
                OnPropertyChanged(nameof(ExPl));
                OnPropertyChanged(nameof(ExEn));

                // Wróć do poprzedniego widoku (np. _onSaved używane do nawigacji)
                _onSaved?.Invoke();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
