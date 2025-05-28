using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
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

        /* ---------- pobierz wszystkie kursy ---------- */
        public IEnumerable<Course> GetCourses()
        {
            var cmd = new SQLiteCommand("SELECT id, name FROM courses", _connection);
            using (SQLiteDataReader reader = cmd.ExecuteReader())
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

        /* ---------- sprawdź, czy nazwa kursu już istnieje (case-insensitive) ---------- */
        public bool CourseNameExists(string name)
        {
            var cmd = new SQLiteCommand(
                "SELECT COUNT(1) FROM courses WHERE lower(name) = lower(@n)",
                _connection);
            cmd.Parameters.AddWithValue("@n", name);
            object result = cmd.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }

        /* ---------- dodaj kurs, zwróć jego nowe ID ---------- */
        public int AddCourse(string name)
        {
            var cmd = new SQLiteCommand(
                "INSERT INTO courses (name) VALUES (@n); SELECT last_insert_rowid();",
                _connection);
            cmd.Parameters.AddWithValue("@n", name);
            object scalar = cmd.ExecuteScalar();
            return Convert.ToInt32(scalar);
        }

        /* ---------- pobierz słowa dla danego kursu ---------- */
        public IEnumerable<Word> GetWords(int courseId)
        {
            var cmd = new SQLiteCommand(
                @"SELECT 
                    id, word_pl, word_en, example_pl, example_en,
                    correct_count, category, next_review_date
                  FROM words
                  WHERE course_id = @cid
                  ORDER BY id",
                _connection);
            cmd.Parameters.AddWithValue("@cid", courseId);

            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    yield return new Word
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        CourseId = courseId,
                        WordPl = reader["word_pl"].ToString(),
                        WordEn = reader["word_en"].ToString(),
                        ExamplePl = reader["example_pl"] == DBNull.Value ? null : reader["example_pl"].ToString(),
                        ExampleEn = reader["example_en"] == DBNull.Value ? null : reader["example_en"].ToString(),
                        CorrectCount = Convert.ToInt32(reader["correct_count"]),
                        Category = reader["category"].ToString(),
                        NextReviewDate = reader["next_review_date"] == DBNull.Value
                                         ? (DateTime?)null
                                         : DateTime.Parse(reader["next_review_date"].ToString())
                    };
                }
            }
        }

        /* ---------- dodaj słowo i przypisz jego ID ---------- */
        public void AddWord(Word w)
        {
            var cmd = new SQLiteCommand(
                @"INSERT INTO words
                    (course_id, word_pl, word_en, example_pl, example_en, category, correct_count, next_review_date)
                  VALUES
                    (@cid, @pl, @en, @expl, @exen, @cat, @cc, NULL);
                  SELECT last_insert_rowid();",
                _connection);
            cmd.Parameters.AddWithValue("@cid", w.CourseId);
            cmd.Parameters.AddWithValue("@pl", w.WordPl);
            cmd.Parameters.AddWithValue("@en", w.WordEn);
            cmd.Parameters.AddWithValue("@expl", string.IsNullOrWhiteSpace(w.ExamplePl) ? (object)DBNull.Value : w.ExamplePl);
            cmd.Parameters.AddWithValue("@exen", string.IsNullOrWhiteSpace(w.ExampleEn) ? (object)DBNull.Value : w.ExampleEn);
            cmd.Parameters.AddWithValue("@cat", w.Category);
            cmd.Parameters.AddWithValue("@cc", w.CorrectCount);

            // Pobierz nowo wstawione ID i przypisz
            var scalar = cmd.ExecuteScalar();
            w.Id = Convert.ToInt32(scalar);
        }

        // 1) Aktualizacja nazwy kursu
        public void UpdateCourseName(int courseId, string newName)
        {
            var cmd = new SQLiteCommand(
                "UPDATE courses SET name = @n WHERE id = @id", _connection);
            cmd.Parameters.AddWithValue("@n", newName);
            cmd.Parameters.AddWithValue("@id", courseId);
            cmd.ExecuteNonQuery();
        }

        // 2) Usunięcie słowa
        public void DeleteWord(int wordId)
        {
            var cmd = new SQLiteCommand(
                "DELETE FROM words WHERE id = @id", _connection);
            cmd.Parameters.AddWithValue("@id", wordId);
            cmd.ExecuteNonQuery();
        }

        // 3) Usuń wszystkie słowa z kursu poza keepIds
        public void DeleteWordsNotInCourse(int courseId, List<int> keepIds)
        {
            string sql;
            if (keepIds.Any())
                sql = $"DELETE FROM words WHERE course_id = @cid AND id NOT IN ({string.Join(",", keepIds)})";
            else
                sql = "DELETE FROM words WHERE course_id = @cid";

            using var cmd = new SQLiteCommand(sql, _connection);
            cmd.Parameters.AddWithValue("@cid", courseId);
            cmd.ExecuteNonQuery();
        }

        // 4) Aktualizacja słowa
        public void UpdateWord(Word w)
        {
            var cmd = new SQLiteCommand(@"
        UPDATE words 
           SET word_pl     = @pl,
               word_en     = @en,
               example_pl  = @expl,
               example_en  = @exen
         WHERE id          = @id", _connection);
            cmd.Parameters.AddWithValue("@pl", w.WordPl);
            cmd.Parameters.AddWithValue("@en", w.WordEn);
            cmd.Parameters.AddWithValue("@expl", (object)w.ExamplePl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@exen", (object)w.ExampleEn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", w.Id);
            cmd.ExecuteNonQuery();
        }

        public void DeleteCourse(int courseId)
        {
            // 1) Usuń wszystkie słowa z tego kursu
            var cmdDelWords = new SQLiteCommand(
                "DELETE FROM words WHERE course_id = @cid", _connection);
            cmdDelWords.Parameters.AddWithValue("@cid", courseId);
            cmdDelWords.ExecuteNonQuery();

            // 2) Usuń sam kurs
            var cmdDelCourse = new SQLiteCommand(
                "DELETE FROM courses WHERE id = @cid", _connection);
            cmdDelCourse.Parameters.AddWithValue("@cid", courseId);
            cmdDelCourse.ExecuteNonQuery();
        }

        public IEnumerable<Word> GetWordsSortedAlphabetically(int courseId)
        {
            var cmd = new SQLiteCommand(
                @"SELECT 
             id, word_pl, word_en, example_pl, example_en,
             correct_count, category, next_review_date
          FROM words
          WHERE course_id = @cid
          ORDER BY word_pl COLLATE NOCASE",   // sortowanie alfabetyczne
                _connection);
            cmd.Parameters.AddWithValue("@cid", courseId);

            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    yield return new Word
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        CourseId = courseId,
                        WordPl = reader["word_pl"].ToString(),
                        WordEn = reader["word_en"].ToString(),
                        ExamplePl = reader["example_pl"] as string,
                        ExampleEn = reader["example_en"] as string,
                        CorrectCount = Convert.ToInt32(reader["correct_count"]),
                        Category = reader["category"].ToString(),
                        NextReviewDate = reader["next_review_date"] == DBNull.Value
                                         ? (DateTime?)null
                                         : DateTime.Parse(reader["next_review_date"].ToString())
                    };
        }

        public IEnumerable<StatsEntry> GetStats(int courseId, DateTime start, DateTime end)
        {
            var list = new List<StatsEntry>();
            using (var con = new SQLiteConnection("Data Source=Data/app.db;Version=3;"))
            {
                con.Open();
                var cmd = new SQLiteCommand(@"
            SELECT
                wpl.practice_date AS date,
                COUNT(*) AS total,
                SUM(CASE WHEN w.correct_count > 0 THEN 1 ELSE 0 END) AS correct
            FROM word_practice_log wpl
            JOIN words w ON wpl.word_id = w.id
            WHERE w.course_id = @cid
              AND wpl.practice_date BETWEEN @start AND @end
            GROUP BY wpl.practice_date
            ORDER BY wpl.practice_date", con);
                cmd.Parameters.AddWithValue("@cid", courseId);
                cmd.Parameters.AddWithValue("@start", start.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@end", end.ToString("yyyy-MM-dd"));
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new StatsEntry
                    {
                        Date = DateTime.Parse(reader["date"].ToString()),
                        TotalQuestions = Convert.ToInt32(reader["total"]),
                        CorrectAnswers = Convert.ToInt32(reader["correct"])
                    });
                }
            }
            return list;
        }
    }
}
