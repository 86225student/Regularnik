using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using Regularnik.Models;
using Regularnik.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Regularnik.ViewModels
{
    public class StatisticsViewModel : ObservableObject
    {
        private readonly DatabaseService _db;

        public ObservableCollection<Course> Courses { get; }
        public Course SelectedCourse { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-7);
        public DateTime EndDate { get; set; } = DateTime.Today;

        public PlotModel PlotModel { get; private set; }
        public ICommand LoadStatsCommand { get; }
        private bool _isPlotVisible;
        public bool IsPlotVisible
        {
            get => _isPlotVisible;
            set
            {
                _isPlotVisible = value;
                OnPropertyChanged(nameof(IsPlotVisible));
            }
        }


        public StatisticsViewModel(DatabaseService db)
        {
            _db = db;
            Courses = new ObservableCollection<Course>(_db.GetCourses());
            LoadStatsCommand = new RelayCommand(_ => LoadStatistics());
        }

        private void LoadStatistics()
        {
            IsPlotVisible = true;
            if (SelectedCourse == null) return;

            var stats = _db.GetStats(SelectedCourse.Id, StartDate, EndDate).ToList();

            var model = new PlotModel { Title = $"Statystyki: {SelectedCourse.Name}",
                Background = OxyColor.Parse("#1C2833"), // tło całego wykresu (ramka, legenda)
                TextColor = OxyColors.White,            // kolor tekstów (osi, tytułów itd.)
                PlotAreaBackground = OxyColor.Parse("#1C2833") // tło obszaru z wykresem (np. słupk)
            };

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                Title = "Data",
                ItemsSource = stats,
                LabelField = "DateLabel",
                IsZoomEnabled = false,
                IsPanEnabled = false,
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.White,
            };

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Liczba pytań",
                IsZoomEnabled = false,
                IsPanEnabled = false,
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.White,
            };

            var totalSeries = new BarSeries
            {
                Title = "Wszystkie",
                FillColor = OxyColors.Gold,
                ItemsSource = stats,
                ValueField = "TotalQuestions"
            };

            var correctSeries = new BarSeries
            {
                Title = "Poprawne",
                FillColor = OxyColors.Green,
                ItemsSource = stats,
                ValueField = "CorrectAnswers"
            };

            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);
            model.Series.Add(totalSeries);
            model.Series.Add(correctSeries);

            PlotModel = model;
            OnPropertyChanged(nameof(PlotModel));
        }

    }

    public class StatsEntry
    {
        public DateTime Date { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
    }
}
