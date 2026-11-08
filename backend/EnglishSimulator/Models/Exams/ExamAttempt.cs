
namespace EnglishSimulator.Models.Exams;

public class ExamAttempt
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ExamType { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? FinishedAt { get; set; }

    public decimal Score { get; set; }

    public bool IsPassed { get; set; }
}
