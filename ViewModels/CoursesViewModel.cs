using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Regularnik.Models;
using Regularnik.Services;

namespace Regularnik.ViewModels
{
    public class CoursesViewModel
    {
        private readonly DatabaseService _db;
        private readonly Action<Course> _onCourseSelected;

        public ObservableCollection<Course> Courses { get; } =
            new ObservableCollection<Course>();

        public ICommand SelectCourseCommand { get; }

        public CoursesViewModel(DatabaseService db, Action<Course> onCourseSelected)
        {
            _db = db;
            _onCourseSelected = onCourseSelected;

            // ładujemy kursy do kolekcji
            foreach (var c in _db.GetCourses())
                Courses.Add(c);

            SelectCourseCommand = new RelayCommand(c =>
            {
                _onCourseSelected?.Invoke(c as Course);
            });
        }
    }
}