using System.Collections.ObjectModel;
using System.Windows.Input;
using Regularnik.Models;
using Regularnik.Services;

namespace Regularnik.ViewModels
{
    public class CatalogViewModel
    {
        private readonly DatabaseService _db;
        private readonly System.Action<Course> _onCourseSelected;
        private readonly System.Action _onNewCourse;

        public ObservableCollection<Course> Courses { get; } =
            new ObservableCollection<Course>();

        public ICommand SelectCourseCommand { get; }
        public ICommand NewCourseCommand { get; }

        public CatalogViewModel(DatabaseService db,
                                System.Action<Course> onCourseSelected,
                                System.Action onNewCourse)
        {
            _db = db;
            _onCourseSelected = onCourseSelected;
            _onNewCourse = onNewCourse;

            LoadCourses();

            SelectCourseCommand = new RelayCommand(c =>
            {
                _onCourseSelected?.Invoke(c as Course);
            });

            NewCourseCommand = new RelayCommand(_ => _onNewCourse?.Invoke());
        }

        /* ---------- dane ---------- */
        public void LoadCourses()
        {
            Courses.Clear();
            foreach (var c in _db.GetCourses())
                Courses.Add(c);
        }
    }
}
