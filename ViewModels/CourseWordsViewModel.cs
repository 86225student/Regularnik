using System.Collections.ObjectModel;
using Regularnik.Models;
using Regularnik.Services;

namespace Regularnik.ViewModels
{
    public class CourseWordsViewModel
    {
        public string Header { get; }
        public ObservableCollection<Word> Words { get; } =
            new ObservableCollection<Word>();

        public CourseWordsViewModel(DatabaseService db, Course course)
        {
            // „Kurs: nazwa”
            Header = $"Kurs: {course.Name}";

            foreach (var w in db.GetWords(course.Id))
                Words.Add(w);
        }
    }
}
