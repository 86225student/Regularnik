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

        //private void LoadStatistics()
        //{
        //    IsPlotVisible = true;
        //    if (SelectedCourse == null) return;

        //    var stats = _db.GetStats(SelectedCourse.Id, StartDate, EndDate).ToList();

        //    var model = new PlotModel { Title = $"Statystyki: {SelectedCourse.Name}",
        //        Background = OxyColor.Parse("#1C2833"), // tło całego wykresu (ramka, legenda)
        //        TextColor = OxyColors.White,            // kolor tekstów (osi, tytułów itd.)
        //        PlotAreaBackground = OxyColor.Parse("#1C2833") // tło obszaru z wykresem (np. słupk)
        //    };

        //    var categoryAxis = new CategoryAxis
        //    {
        //        Position = AxisPosition.Bottom, // teraz na dole
        //        Title = "Data",
        //        ItemsSource = stats,
        //        LabelField = "DateLabel",
        //        IsZoomEnabled = false,
        //        IsPanEnabled = false,
        //        TitleColor = OxyColors.White,
        //        TextColor = OxyColors.White,
        //        TicklineColor = OxyColors.White,
        //    };

        //    var valueAxis = new LinearAxis
        //    {
        //        Position = AxisPosition.Left, // teraz po lewej
        //        Title = "Liczba pytań",
        //        IsZoomEnabled = false,
        //        IsPanEnabled = false,
        //        TitleColor = OxyColors.White,
        //        TextColor = OxyColors.White,
        //        TicklineColor = OxyColors.White,
        //    };

        //    var totalSeries = new ColumnSeries
        //    {
        //        Title = "Wszystkie",
        //        FillColor = OxyColors.Gold,
        //        ItemsSource = null, // usuwamy
        //        Items = stats.Select(s => new ColumnItem(s.TotalQuestions)).ToList()
        //    };


        //    var correctSeries = new ColumnSeries
        //    {
        //        Title = "Poprawne",
        //        FillColor = OxyColors.Green,
        //        ItemsSource = null,
        //        Items = stats.Select(s => new ColumnItem(s.CorrectAnswers)).ToList()
        //    };

        //    model.Axes.Add(categoryAxis);
        //    model.Axes.Add(valueAxis);
        //    model.Series.Add(totalSeries);
        //    model.Series.Add(correctSeries);

        //    PlotModel = model;
        //    OnPropertyChanged(nameof(PlotModel));
        //}
        private void LoadStatistics()
        {
            IsPlotVisible = true;
            if (SelectedCourse == null) return;

            var stats = _db.GetStats(SelectedCourse.Id, StartDate, EndDate).ToList();

            var model = new PlotModel
            {
                Title = $"Statystyki: {SelectedCourse.Name}",
                Background = OxyColors.Transparent, // wersja 1.0.0 nie ma Parse
                TextColor = OxyColors.White,
                PlotAreaBackground = OxyColors.Transparent
            };

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Data",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                IsZoomEnabled = false,
                IsPanEnabled = false
            };

            // Dodaj etykiety ręcznie
            foreach (var entry in stats)
            {
                categoryAxis.Labels.Add(entry.Date.ToString("dd.MM"));
            }

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Liczba pytań",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                IsZoomEnabled = false,
                IsPanEnabled = false
            };

            var totalSeries = new ColumnSeries
            {
                Title = "Wszystkie",
                FillColor = OxyColors.Gold
            };
            totalSeries.Items.AddRange(stats.Select(s => new ColumnItem(s.TotalQuestions)));

            var correctSeries = new ColumnSeries
            {
                Title = "Poprawne",
                FillColor = OxyColors.Green
            };
            correctSeries.Items.AddRange(stats.Select(s => new ColumnItem(s.CorrectAnswers)));

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
