using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regularnik.Models
{
    public class Word
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string WordPl { get; set; }
        public string WordEn { get; set; }
        public string ExamplePl { get; set; }
        public string ExampleEn { get; set; }
        public string Category { get; set; }
        public int CorrectCount { get; set; }
        public DateTime? NextReviewDate { get; set; }
    }
}
