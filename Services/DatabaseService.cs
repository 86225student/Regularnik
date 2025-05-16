using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regularnik.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using Regularnik.Models;
    using System.Data;
    using System.Data.SQLite;

    public class DatabaseService
    {
        private readonly SQLiteConnection _connection;

        public DatabaseService()
        {
            
            _connection = new SQLiteConnection("Data Source=Data/app.db;Version=3;");
            _connection.Open();
        }

        public IEnumerable<Course> GetCourses()
        {
            var cmd = new SQLiteCommand("SELECT id, name FROM courses", _connection);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    yield return new Course
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Name = reader["name"].ToString()
                    };
                }
            }
        }
    }
}
