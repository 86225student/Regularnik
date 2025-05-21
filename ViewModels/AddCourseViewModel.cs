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
        private int _tempCourseId = -1;

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

        public AddCourseViewModel(DatabaseService db, Action onSaved)
        {
            _db = db;
            _onSaved = onSaved;

            AddWordCommand = new RelayCommand(_ =>
            {
                if (string.IsNullOrWhiteSpace(CourseName))
                {
                    MessageBox.Show("Musisz podać nazwę kursu.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string trimmedName = CourseName.Trim();
                if (_tempCourseId == -1 && _db.CourseNameExists(trimmedName))
                {
                    MessageBox.Show($"Kurs „{trimmedName}” już istnieje.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(WordPl) || string.IsNullOrWhiteSpace(WordEn))
                {
                    MessageBox.Show("Podaj słowo po polsku i po angielsku.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_tempCourseId == -1)
                {
                    _tempCourseId = _db.AddCourse(trimmedName);
                }

                var w = new Word
                {
                    CourseId = _tempCourseId,
                    WordPl = WordPl.Trim(),
                    WordEn = WordEn.Trim(),
                    ExamplePl = string.IsNullOrWhiteSpace(ExPl) ? null : ExPl.Trim(),
                    ExampleEn = string.IsNullOrWhiteSpace(ExEn) ? null : ExEn.Trim(),
                    Category = "NOWE",
                    CorrectCount = 0,
                    NextReviewDate = null
                };

                _db.AddWord(w);
                TempWords.Add(w);

                WordPl = WordEn = ExPl = ExEn = string.Empty;
            });

            SaveCourseCommand = new RelayCommand(_ =>
            {
                if (_tempCourseId == -1)
                {
                    MessageBox.Show("Dodaj przynajmniej jedno słowo.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _onSaved?.Invoke();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
