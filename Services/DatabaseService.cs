using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Regularnik.Models;

namespace Regularnik.Services
{
    public class DatabaseService
    {
        private readonly SQLiteConnection _connection;

        public DatabaseService()
        {
            _connection = new SQLiteConnection("Data Source=Data/app.db;Version=3;");
            _connection.Open();
        }

        /* ---------- kursy ---------- */
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

        /// <summary>
        /// Dodaje kurs i zwraca wygenerowane ID.
        /// </summary>
        public int AddCourse(string name)
        {
            var cmd = new SQLiteCommand(
                "INSERT INTO courses (name) VALUES (@n); SELECT last_insert_rowid();",
                _connection);

            cmd.Parameters.AddWithValue("@n", name);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /* ---------- słówka ---------- */
        public IEnumerable<Word> GetWords(int courseId)
        {
            var cmd = new SQLiteCommand(
                "SELECT id, word_pl, word_en FROM words WHERE course_id = @cid",
                _connection);

            cmd.Parameters.AddWithValue("@cid", courseId);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    yield return new Word
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        WordPl = reader["word_pl"].ToString(),
                        WordEn = reader["word_en"].ToString()
                    };
                }
            }
        }

        /// <summary>
        /// Dodaje słowo do tabeli words.
        /// category = NOWE, correct_count = 0, next_review_date = NULL
        /// </summary>
        public void AddWord(Word w)
        {
            var cmd = new SQLiteCommand(
                @"INSERT INTO words
                    (course_id, word_pl, word_en, example_pl, example_en,
                     category, correct_count, next_review_date)
                  VALUES
                    (@cid, @pl, @en, @expl, @exen, @cat, @cc, NULL)",
                _connection);

            cmd.Parameters.AddWithValue("@cid", w.CourseId);
            cmd.Parameters.AddWithValue("@pl", w.WordPl);
            cmd.Parameters.AddWithValue("@en", w.WordEn);
            cmd.Parameters.AddWithValue("@expl", string.IsNullOrWhiteSpace(w.ExamplePl) ? (object)DBNull.Value : w.ExamplePl);
            cmd.Parameters.AddWithValue("@exen", string.IsNullOrWhiteSpace(w.ExampleEn) ? (object)DBNull.Value : w.ExampleEn);
            cmd.Parameters.AddWithValue("@cat", "NOWE");
            cmd.Parameters.AddWithValue("@cc", 0);

            cmd.ExecuteNonQuery();
        }
    }
}
