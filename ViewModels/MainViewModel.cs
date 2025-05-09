
namespace Regularnik.ViewModels
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using Regularnik.Models;
    using Regularnik.Services;

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        public ObservableCollection<Course> Courses { get; }
        public ICommand LoadCoursesCommand { get; }

        public MainViewModel()
        {
            _dbService = new DatabaseService();
            Courses = new ObservableCollection<Course>();
            LoadCoursesCommand = new RelayCommand(_ => LoadCourses());
        }

        private void LoadCourses()
        {
            Courses.Clear();
            foreach (var course in _dbService.GetCourses())
                Courses.Add(course);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
