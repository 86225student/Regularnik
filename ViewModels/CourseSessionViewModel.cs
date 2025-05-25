using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Input;
using Regularnik.Models;
using Regularnik.Services;

namespace Regularnik.ViewModels
{
    public enum Category { New, Reinforce, Review }
    public enum CardStage { Menu, Front, Back, Done }

    public class CourseSessionViewModel : ObservableObject
    {
        /* ===== POLA I DI ===== */
        private readonly DatabaseService _db;
        private readonly Course _course;
        private readonly DateTime _today = DateTime.Today;

        private readonly Queue<Word> _queue = new();
        private Word _current;

        private Category _activeCat;
        private CardStage _stage = CardStage.Menu;
        private bool _showPl;

        /* ====== WŁASNOŚCI ====== */
        public string Header => $"Kurs – {_course.Name}";

        public Category ActiveCat
        {
            get => _activeCat;
            private set { _activeCat = value; OnPropertyChanged(); }
        }

        public CardStage Stage
        {
            get => _stage;
            private set { _stage = value; OnPropertyChanged(); }
        }

        public bool ShowPolishExample
        {
            get => _showPl;
            private set { _showPl = value; OnPropertyChanged(); }
        }

        public string WordPl => _current?.WordPl;
        public string WordEn => _current?.WordEn;
        public string ExPl => _current?.ExamplePl;
        public string ExEn => _current?.ExampleEn;

        public int NewCount { get; private set; }
        public int ReinforceCount { get; private set; }
        public int ReviewCount { get; private set; }

        public string NewLabel => $"NOWE ({NewCount})";
        public string ReinforceLabel => $"UTRWAL ({ReinforceCount})";
        public string ReviewLabel => $"POWTÓRKI ({ReviewCount})";

        /* ===== KOMENDY ===== */
        public ICommand StartNewCommand { get; }
        public ICommand StartReinforceCommand { get; }
        public ICommand StartReviewCommand { get; }
        public ICommand ShowBackCommand { get; }
        public ICommand ToggleTranslateCmd { get; }
        public ICommand KnowCmd { get; }
        public ICommand AlmostCmd { get; }
        public ICommand DontKnowCmd { get; }

        /* ===== KONSTRUKTOR ===== */
        public CourseSessionViewModel(DatabaseService db, Course course)
        {
            _db = db;
            _course = course;

            StartNewCommand = new RelayCommand(_ => Load(Category.New));
            StartReinforceCommand = new RelayCommand(_ => Load(Category.Reinforce));
            StartReviewCommand = new RelayCommand(_ => Load(Category.Review));

            ShowBackCommand = new RelayCommand(_ =>
            {
                Stage = CardStage.Back;
                ShowPolishExample = false;
            });

            ToggleTranslateCmd = new RelayCommand(_ => ShowPolishExample = !ShowPolishExample);

            KnowCmd = new RelayCommand(_ => Commit(Answer.Know));
            AlmostCmd = new RelayCommand(_ => Commit(Answer.Almost));
            DontKnowCmd = new RelayCommand(_ => Commit(Answer.Nope));

            RefreshCounts();
        }

        /* ===== ŁADOWANIE SŁÓWEK ===== */
        private void Load(Category cat)
        {
            ActiveCat = cat;
            _queue.Clear();

            foreach (var w in _db.GetWords(_course.Id))
            {
                switch (cat)
                {
                    case Category.New when w.CorrectCount == 0:
                    case Category.Reinforce when w.CorrectCount > 0 &&
                           w.NextReviewDate?.Date <= _today &&
                           (w.Category == "Reinforce" || w.Category == "New"):
                    case Category.Review when w.CorrectCount > 0 &&
                           w.NextReviewDate?.Date <= _today &&
                           w.Category == "Review":
                        _queue.Enqueue(w);
                        break;
                }
            }

            Next();
        }

        /* ===== OBSŁUGA KART ===== */
        private void Next()
        {
            if (_queue.Count == 0)
            {
                Stage = CardStage.Done;
                _current = null;
                NotifyWord();
                RefreshCounts();
                return;
            }

            _current = _queue.Dequeue();
            Stage = CardStage.Front;
            ShowPolishExample = false;
            NotifyWord();
        }

        private void NotifyWord()
        {
            OnPropertyChanged(nameof(WordPl));
            OnPropertyChanged(nameof(WordEn));
            OnPropertyChanged(nameof(ExPl));
            OnPropertyChanged(nameof(ExEn));
        }

        /* ===== ODPOWIEDZI UŻYTKOWNIKA ===== */
        private enum Answer { Know, Almost, Nope }

        private void Commit(Answer ans)
        {
            if (_current == null) return;

            switch (ans)
            {
                case Answer.Know:
                    _current.CorrectCount++;
                    _current.Category = "Review";
                    _current.NextReviewDate = _today.AddDays(_current.CorrectCount * 3 + 1);
                    break;

                case Answer.Almost:
                    _current.CorrectCount = _current.CorrectCount * 2 + 1;
                    _current.Category = "Reinforce";
                    _current.NextReviewDate = _today;
                    break;

                case Answer.Nope:
                    _current.CorrectCount = Math.Max(1, _current.CorrectCount * 2 + 1);
                    _current.Category = "Reinforce";
                    _current.NextReviewDate = _today;
                    break;
            }

            SaveWord(_current);

            /* ------ kluczowa linia ------ */
            RefreshCounts();          // ← aktualizujemy liczniki NATYCHMIAST
            /* --------------------------- */

            Next();
        }

        private void SaveWord(Word w)
        {
            using var con = new SQLiteConnection("Data Source=Data/app.db;Version=3;");
            con.Open();

            const string sql = @"UPDATE words
                                 SET correct_count    = @cc,
                                     next_review_date = @nrd,
                                     category         = @cat
                                 WHERE id = @id";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@cc", w.CorrectCount);
            cmd.Parameters.AddWithValue("@nrd", w.NextReviewDate?.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@cat", w.Category);
            cmd.Parameters.AddWithValue("@id", w.Id);
            cmd.ExecuteNonQuery();
        }

        /* ===== LICZNIKI ===== */
        private void RefreshCounts()
        {
            var words = _db.GetWords(_course.Id).ToList();

            NewCount = words.Count(w => w.CorrectCount == 0);
            ReinforceCount = words.Count(w =>
                w.CorrectCount > 0 &&
                w.NextReviewDate?.Date <= _today &&
                (w.Category == "Reinforce" || w.Category == "New"));
            ReviewCount = words.Count(w =>
                w.CorrectCount > 0 &&
                w.NextReviewDate?.Date <= _today &&
                w.Category == "Review");

            OnPropertyChanged(nameof(NewCount));
            OnPropertyChanged(nameof(ReinforceCount));
            OnPropertyChanged(nameof(ReviewCount));
            OnPropertyChanged(nameof(NewLabel));
            OnPropertyChanged(nameof(ReinforceLabel));
            OnPropertyChanged(nameof(ReviewLabel));
        }
    }
}
