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

        public ObservableCollection<Course> Courses { get; } =
            new ObservableCollection<Course>();

        public ICommand SelectCourseCommand { get; }

        public CatalogViewModel(DatabaseService db, System.Action<Course> onCourseSelected)
        {
            _db = db;
            _onCourseSelected = onCourseSelected;

            LoadCourses();

            SelectCourseCommand = new RelayCommand(c =>
            {
                System.Diagnostics.Debug.WriteLine($"► Klik kurs: {((Course)c).Name}");
                _onCourseSelected?.Invoke(c as Course);
            });
        }

        /* ---------- dane ---------- */
        private void LoadCourses()
        {
            Courses.Clear();
            foreach (var c in _db.GetCourses())
                Courses.Add(c);
        }
    }
}
