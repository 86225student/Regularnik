using System;

namespace Regularnik.Models;

public class StatsEntry
{
    public DateTime Date { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }

    public string DateLabel => Date.ToString("dd.MM"); // <-- TO JEST KLUCZOWE
}

