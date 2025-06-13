using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Regularnik.Models;
using Regularnik.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Regularnik.ViewModels
{
    public class StatisticsViewModel : ObservableObject
    {
        private readonly DatabaseService _db;

        public ObservableCollection<Course> Courses { get; }
        public Course SelectedCourse { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-7);
        public DateTime EndDate { get; set; } = DateTime.Today;

        private ImageSource _plotImage;
        public ImageSource PlotImage
        {
            get => _plotImage;
            set
            {
                _plotImage = value;
                OnPropertyChanged(nameof(PlotImage));
            }
        }

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

            var model = new PlotModel { Title = $"Statystyki: {SelectedCourse.Name}" };

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Data"
            };

            foreach (var entry in stats)
            {
                categoryAxis.Labels.Add(entry.Date.ToString("dd.MM"));
            }

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Liczba pytań"
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

            // Renderowanie wykresu jako obraz
            using (var stream = new MemoryStream())
            {
                var exporter = new OxyPlot.WindowsForms.PngExporter
                {
                    Width = 700,
                    Height = 400,
                    Background = OxyColors.White
                };

                exporter.Export(model, stream);
                stream.Seek(0, SeekOrigin.Begin);

                var bitmap = new PngBitmapDecoder(
                    stream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad
                ).Frames[0];

                PlotImage = bitmap;
            }
        }
    }

    public class StatsEntry
    {
        public DateTime Date { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
    }
}
